import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { GcodeParserComponent } from './gcode-parser/gcode-parser.component';
import { GcodeUploaderComponent } from './gcode-uploader/gcode-uploader.component';
import { GcodeService } from './services/gcode.service';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    GcodeParserComponent,
    GcodeUploaderComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'gcode-parser', component: GcodeParserComponent },
    ])
  ],
  providers: [GcodeService],
  bootstrap: [AppComponent]
})
export class AppModule { }
