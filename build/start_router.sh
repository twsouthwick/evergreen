#!/bin/bash

set -eu

export PATH="$PATH:/openils/bin"

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

echo "[opensrf-docker] starting OpenSRF services"
touch /openils/var/log/osrfsys.log
osrf_control --start --service router
touch /openils/started
scanpids &
tail -f  /openils/var/log/osrfsys.log
echo "Completed"