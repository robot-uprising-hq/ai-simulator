#!/bin/bash

AIREMOTEBRAINFOLDER="../ai-remote-brain/Assets/BrainFileToUse"
SIMULATORBRAINFILEBASE="results"
# ===
# === TELL WHAT'S GONNA HAPPEN ===
# ===
echo
echo "=== This script moves user specified brain file to "
echo "=== AI Brain Server's 'Assets/BrainFileToUse'-folder."
echo "=== The brain file is taken automatically into use"
echo "=== when AI Brain Server starts."
echo "=== The script asks to automatically remove existing"
echo "=== files in the folder if any are found."
echo
echo "=== AI Brain Server's brain is expected to be"
echo "=== in folder $AIREMOTEBRAINFOLDER"
echo "========================================================="
echo


# ===
# === ASK THE RUNID FROM WHICH TO TAKE THE BRAIN FILE INTO USE ===
# ===
echo "=== Available runIDs"
find ./results -maxdepth 1 -mindepth 1 -type d -printf '%f\n'
echo "==="
read -p "Which runID's brain file to move to AI Remote Brain? [Give the name] -> " RUNID

if [ ! -d "$SIMULATORBRAINFILEBASE/$RUNID" ]
then
    echo "=== runID called '$RUNID' does not exits. Exiting..."
    exit 1
fi

BRAINFILEPATH=$(find ./$SIMULATORBRAINFILEBASE/$RUNID -maxdepth 1 -type f -name "*.nn")
if [ ! -f "$BRAINFILEPATH" ]
then
    echo "=== Could not find brainfile from '$SIMULATORBRAINFILEBASE/$RUNID'. Exiting..."
    exit 1
fi
BRAINFILENAME="$(basename $BRAINFILEPATH)"


# ===
# === ASK TO DELETE THE EXISTING MODEL(S) ===
# ===
if [ "$(ls $AIREMOTEBRAINFOLDER)" ]; then
    echo
    echo
    echo "!!!!! $AIREMOTEBRAINFOLDER-folder is not empty !!!!!"
    echo
    read -p "Remove all files in that folder? [y=yes, N=quit]" response
    if [[ "$response" =~ ^([yY])$ ]]
    then
        rm $AIREMOTEBRAINFOLDER/*.*
    else
        echo
        echo "==== Exiting ===="
        echo
        exit 1
    fi
    echo
    echo
fi


# ===
# === MOVE THE BRAIN FILE INTO AI REMOTE BRAIN ===
# ===
echo "=== Copying the new brain file to AI Remote Brain"
cp $SIMULATORBRAINFILEBASE/$RUNID/$BRAINFILENAME $AIREMOTEBRAINFOLDER


# ===
# === DONE ===
# ===
echo "=== Done"
