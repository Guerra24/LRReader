#!/bin/bash

# Stolen from https://github.com/OrchidTechnologies/orchid/blob/1d1852e0ad4993a150403d67c5ccb97de527e734/env/relink.sh

set -e

sysroot=$1
shift 1

cd "${sysroot}"

find . -lname '/*' -print0 | while read -r -d $'\0' link; do
    temp=(${link//\// })
    temp=${temp[@]//*/..}
    temp=${temp// /\/}
    temp=${temp#../../}
    temp=${temp}$(readlink "${link}")
    rm -f "${link}"
    ln -svf "${temp}" "${link}"
done
