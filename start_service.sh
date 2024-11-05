#!/bin/bash

set -eu

perl /openils/bin/eg_db_config --update-config --service all \
       --user $POSTGRES_USER --password $POSTGRES_PASSWORD --hostname $POSTGRES_HOST --port $POSTGRES_PORT \
       --database $POSTGRES_DB

scanpids(){
	# loop and watch pidfiles for missing processes
	PIDFILES="$(find /openils/var/run -type f -name '*.pid')"

	echo "[opensrf-docker] looping, waiting for PIDs to fail"
	while true
	do
		for PIDFILE in $PIDFILES
		do
			PIDS="$(cat $PIDFILE)"
			for PID in $PIDS
			do
				STATUS=$(ps $PID)
				if [ "$?" -ne 0 ]
				then
					echo "[opensrf-docker] ERROR: Process failed: $(basename $PIDFILE .pid)"
					exit 1
				fi
			done
		done
	done 
}

touch /openils/var/log/osrfsys.log
osrf_control --localhost --start-services
echo "Started services"
scanpids &
tail -f  /openils/var/log/osrfsys.log
echo "Completed"