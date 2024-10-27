#!/bin/bash

set -eu

wait_ejabberd(){
	# Wait for ejabberd
	echo "[opensrf-docker] Waiting for ejabberd private network..."
	while ! nc -z private.ejabberd 5222; do
		sleep 1
	done
	echo "[opensrf-docker] private.ejabberd found"

	echo "[opensrf-docker] Waiting for ejabberd public network..."
	while ! nc -z public.ejabberd 5222; do
		sleep 1
	done
	echo "[opensrf-docker] public.ejabberd found"

	echo "[opensrf-docker] giving ejabberd users 10s to get registered"
	sleep 10
	# Start the first process
	echo "[opensrf-docker] starting OpenSRF services"
}

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

wait_ejabberd
touch /openils/var/log/osrfsys.log
osrf_control --start --service router
touch /openils/started
scanpids &
tail -f  /openils/var/log/osrfsys.log
echo "Completed"