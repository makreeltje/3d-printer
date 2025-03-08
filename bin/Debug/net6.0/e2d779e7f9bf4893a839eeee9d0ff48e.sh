function list_child_processes () {
    local ppid=$1;
    local current_children=$(pgrep -P $ppid);
    local local_child;
    if [ $? -eq 0 ];
    then
        for current_child in $current_children
        do
          local_child=$current_child;
          list_child_processes $local_child;
          echo $local_child;
        done;
    else
      return 0;
    fi;
}

ps 25013;
while [ $? -eq 0 ];
do
  sleep 1;
  ps 25013 > /dev/null;
done;

for child in $(list_child_processes 25308);
do
  echo killing $child;
  kill -s KILL $child;
done;
rm /Users/makreeltje/Git/Personal/3d-printer/bin/Debug/net6.0/e2d779e7f9bf4893a839eeee9d0ff48e.sh;
