"""
GCODE parsing utilities and slicer-specific parsers.
"""
import re
from abc import ABC, abstractmethod
from datetime import timedelta
from typing import List, Optional, Dict
from models import ParsedGcode

class IGcodeParser(ABC):
    """Interface for GCODE parsers."""
    
    @abstractmethod
    def can_parse(self, gcode_content: str) -> bool:
        """Check if this parser can handle the given GCODE content."""
        pass
    
    @abstractmethod
    def parse(self, gcode_content: str, filename: str = "") -> ParsedGcode:
        """Parse GCODE content and return structured data."""
        pass

class GcodeParserUtils:
    """Utility functions for GCODE parsing."""
    
    @staticmethod
    def extract_single_float(content: str, marker: str) -> Optional[float]:
        """Extract a single float value following a marker."""
        pattern = rf"{re.escape(marker)}\s*([0-9]*\.?[0-9]+)"
        match = re.search(pattern, content, re.IGNORECASE)
        if match:
            try:
                return float(match.group(1))
            except ValueError:
                return None
        return None
    
    @staticmethod
    def extract_time_from_comment(content: str, patterns: List[str]) -> Optional[timedelta]:
        """Extract print time from comments using multiple patterns."""
        for pattern in patterns:
            match = re.search(pattern, content, re.IGNORECASE | re.MULTILINE)
            if match:
                try:
                    # Default values
                    d = h = m = s = 0

                    # Safely parse groups if they exist and are not empty
                    if match.lastindex and match.lastindex >= 1 and match.group(1):
                        d = int(match.group(1) or 0)
                    if match.lastindex and match.lastindex >= 2 and match.group(2):
                        h = int(match.group(2) or 0)
                    if match.lastindex and match.lastindex >= 3 and match.group(3):
                        m = int(match.group(3) or 0)
                    if match.lastindex and match.lastindex >= 4 and match.group(4):
                        s = int(match.group(4) or 0)

                    return timedelta(days=d, hours=h, minutes=m, seconds=s)

                except (ValueError, AttributeError):
                    continue
        return None

    @staticmethod
    def count_layers(content: str) -> int:
        """Count the number of layers by counting layer change comments."""
        layer_patterns = [
            r";LAYER_CHANGE",
            r";LAYER:",
            r"; layer \d+",
            r"G1 Z\d+\.\d+ F\d+"
        ]
        
        max_count = 0
        for pattern in layer_patterns:
            matches = re.findall(pattern, content, re.IGNORECASE)
            max_count = max(max_count, len(matches))
        
        return max_count

class BambuGcodeParser(IGcodeParser):
    """Parser for Bambu Studio generated GCODE files."""

    def can_parse(self, gcode_content: str) -> bool:
        """Check for Bambu Studio markers."""
        bambu_markers = [
            "BambuStudio"
        ]
        return any(marker in gcode_content for marker in bambu_markers)

    def parse(self, gcode_content: str, filename: str = "") -> ParsedGcode:
        """Parse Bambu Studio GCODE."""
        # Extract filament grams used
        filament_grams_patterns = [
            r";\s*total filament weight \[g\] ?: ?([0-9.]+)",
        ]
        filament_grams = 0.0
        for pattern in filament_grams_patterns:
            match = re.search(pattern, gcode_content, re.IGNORECASE)
            if match:
                try:
                    filament_grams = float(match.group(1))
                    break
                except ValueError:
                    continue

        # Extract filament length
        filament_mm_patterns = [
            r";\s*total filament length \[mm\] ?: ?([0-9.]+)",
        ]

        filament_mm = 0.0
        for pattern in filament_mm_patterns:
            match = re.search(pattern, gcode_content, re.IGNORECASE)
            if match:
                try:
                    filament_mm = float(match.group(1))
                    if filament_mm == 0.0:
                        filament_mm = float(match.group(2))
                    break
                except ValueError:
                    continue

        # Extract print time
        time_patterns = [
            r";\s*estimated printing time.*= ?(?:(\d+)d)? ?(?:(\d+)h)? ?(?:(\d+)m)? ?(?:(\d+)s)?",
        ]
        print_time = GcodeParserUtils.extract_time_from_comment(gcode_content, time_patterns) or timedelta()

        # Extract layer info
        layer_count_patterns = [
            r";\s*total layer number:? ?([0-9]+)",
        ]
        layer_count = 0
        for pattern in layer_count_patterns:
            match = re.search(pattern, gcode_content, re.IGNORECASE)
            if match:
                try:
                    layer_count = float(match.group(1))
                    break
                except ValueError:
                    continue

        return ParsedGcode(
            slicer_name="Bambu Studio",
            file_name=filename,
            estimated_print_time=print_time,
            filament_used_grams=filament_grams,
            filament_used_millimeters=filament_mm,
            layer_count=layer_count,
        )

class OrcaGcodeParser(IGcodeParser):
    """Parser for Orca Slicer generated GCODE files."""

    def can_parse(self, gcode_content: str) -> bool:
        """Check for Orca Slicer markers."""
        orca_markers = [
            "; generated by OrcaSlicer",
            "OrcaSlicer"
        ]
        return any(marker in gcode_content for marker in orca_markers)

    def parse(self, gcode_content: str, filename: str = "") -> ParsedGcode:
        """Parse Orca Slicer GCODE."""
        # Extract filament grams used
        filament_grams_patterns = [
            r";\s*total filament used \[g\] ?= ?([0-9.]+)",
        ]
        filament_grams = 0.0
        for pattern in filament_grams_patterns:
            match = re.search(pattern, gcode_content, re.IGNORECASE)
            if match:
                try:
                    filament_grams = float(match.group(1))
                    break
                except ValueError:
                    continue

        # Extract filament length
        filament_mm_patterns = r";\s*filament used \[mm\]\s*=\s*([0-9.,\s]+)"

        match = re.search(filament_mm_patterns, gcode_content, re.IGNORECASE)

        filament_mm = 0.0
        if match:
            try:
                values = match.group(1).split(',')
                float_values = [float(v.strip()) for v in values if v.strip()]
                non_zero_values = [v for v in float_values if v > 0.0]
                if non_zero_values:
                    filament_mm = max(non_zero_values)  # or sum, first, etc. based on your logic
            except ValueError:
                pass

        # Extract print time
        time_patterns = [
            r";\s*estimated printing time.*= ?(?:(\d+)d)? ?(?:(\d+)h)? ?(?:(\d+)m)? ?(?:(\d+)s)?",
        ]
        print_time = GcodeParserUtils.extract_time_from_comment(gcode_content, time_patterns) or timedelta()

        # Extract layer info
        layer_count_patterns = [
            r";\s*total layers count ?= ?([0-9]+)",
        ]
        layer_count = 0
        for pattern in layer_count_patterns:
            match = re.search(pattern, gcode_content, re.IGNORECASE)
            if match:
                try:
                    layer_count = float(match.group(1))
                    break
                except ValueError:
                    continue

        return ParsedGcode(
            slicer_name="Orca Slicer",
            file_name=filename,
            estimated_print_time=print_time,
            filament_used_grams=filament_grams,
            filament_used_millimeters=filament_mm,
            layer_count=layer_count,
        )

class CrealityGcodeParser(IGcodeParser):
    """Parser for Creality Slicer generated GCODE files."""
    
    def can_parse(self, gcode_content: str) -> bool:
        """Check for Creality Slicer markers."""
        creality_markers = [
            "Creality_Print",
            "; generated by Creality_Print"
        ]
        return any(marker in gcode_content for marker in creality_markers)
    
    def parse(self, gcode_content: str, filename: str = "") -> ParsedGcode:
        """Parse Creality Slicer GCODE."""
        # Extract filament grams used
        filament_grams_patterns = r";\s*filament used \[g\]\s*=\s*([0-9.,\s]+)"

        match = re.search(filament_grams_patterns, gcode_content, re.IGNORECASE)

        filament_grams = 0.0
        if match:
            try:
                values = match.group(1).split(',')
                float_values = [float(v.strip()) for v in values if v.strip()]
                non_zero_values = [v for v in float_values if v > 0.0]
                if non_zero_values:
                    filament_grams = max(non_zero_values)  # or sum, first, etc. based on your logic
            except ValueError:
                pass
        
        # Calculate filament length from extrusion commands if weight not found
        filament_mm_patterns = r";\s*filament used \[mm\]\s*=\s*([0-9.,\s]+)"

        match = re.search(filament_mm_patterns, gcode_content, re.IGNORECASE)

        filament_mm = 0.0
        if match:
            try:
                values = match.group(1).split(',')
                float_values = [float(v.strip()) for v in values if v.strip()]
                non_zero_values = [v for v in float_values if v > 0.0]
                if non_zero_values:
                    filament_mm = max(non_zero_values)  # or sum, first, etc. based on your logic
            except ValueError:
                pass
        
        # Extract print time
        time_patterns = [
            r";\s*estimated printing time.*= ?(?:(\d+)d)? ?(?:(\d+)h)? ?(?:(\d+)m)? ?(?:(\d+)s)?",
        ]
        print_time = GcodeParserUtils.extract_time_from_comment(gcode_content, time_patterns) or timedelta()
        
        # Extract layer info
        layer_count_patterns = [
            r";\s*total layer number:? ?([0-9]+)",
        ]
        layer_count = 0
        for pattern in layer_count_patterns:
            match = re.search(pattern, gcode_content, re.IGNORECASE)
            if match:
                try:
                    layer_count = float(match.group(1))
                    break
                except ValueError:
                    continue

        return ParsedGcode(
            slicer_name="Creality Slicer",
            file_name=filename,
            estimated_print_time=print_time,
            filament_used_grams=filament_grams,
            filament_used_millimeters=filament_mm,
            layer_count=layer_count,
        )

class GenericGcodeParser(IGcodeParser):
    """Generic parser for unknown GCODE formats."""
    
    def can_parse(self, gcode_content: str) -> bool:
        """Always returns True as fallback parser."""
        return True
    
    def parse(self, gcode_content: str, filename: str = "") -> ParsedGcode:
        """Parse GCODE using generic extraction methods."""
        # Try to extract basic info using generic patterns
        layer_count = GcodeParserUtils.count_layers(gcode_content)

        # Generic time patterns
        time_patterns = [
            r"time.*?(\d+)h\s*(\d+)m\s*(\d+)s",
            r"duration.*?(\d+)h\s*(\d+)m\s*(\d+)s",
            r"(\d+)\s*seconds?"
        ]
        print_time = GcodeParserUtils.extract_time_from_comment(gcode_content, time_patterns) or timedelta()
        
        # Calculate filament usage from extrusion
        e_values = re.findall(r"E(\d+\.?\d*)", gcode_content)
        filament_mm = 0.0
        if e_values:
            try:
                filament_mm = max(float(e) for e in e_values)
            except ValueError:
                pass
        
        # Rough conversion to grams (assuming PLA 1.75mm)
        filament_grams = filament_mm * 0.0029 if filament_mm > 0 else 0.0
        
        return ParsedGcode(
            slicer_name="Unknown Slicer",
            file_name=filename,
            estimated_print_time=print_time,
            filament_used_grams=filament_grams,
            filament_used_millimeters=filament_mm,
            layer_count=layer_count,
        )

class GcodeParserFactory:
    """Factory for selecting appropriate GCODE parser."""
    
    def __init__(self):
        self.parsers = [
            BambuGcodeParser(),
            OrcaGcodeParser(),
            CrealityGcodeParser(),
            GenericGcodeParser()  # Always last as fallback
        ]
    
    def get_parser(self, gcode_content: str) -> IGcodeParser:
        """Get the appropriate parser for the given GCODE content."""
        for parser in self.parsers:
            if parser.can_parse(gcode_content):
                return parser
        
        # This should never happen due to GenericGcodeParser fallback
        raise ValueError("No suitable parser found for GCODE content")
    
    def parse_gcode(self, gcode_content: str, filename: str = "") -> ParsedGcode:
        """Parse GCODE content using the appropriate parser."""
        parser = self.get_parser(gcode_content)
        return parser.parse(gcode_content, filename)
