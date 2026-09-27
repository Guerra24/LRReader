#!/bin/bash
set -e

zip -j symbols.zip $BUILD_APP_BIN/*.dbg

rm $BUILD_APP_BIN/*.dbg
rm $BUILD_APP_BIN/*.pdb
