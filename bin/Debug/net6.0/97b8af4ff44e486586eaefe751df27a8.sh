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

ps 28341;
while [ $? -eq 0 ];
do
  sleep 1;
  ps 28341 > /dev/null;
done;

for child in $(list_child_processes 28612);
do
  echo killing $child;
  kill -s KILL $child;
done;
rm /Users/makreeltje/Git/Personal/3d-printer/bin/Debug/net6.0/97b8af4ff44e486586eaefe751df27a8.sh;
