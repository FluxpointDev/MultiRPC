#!/bin/bash

# $1: arch (aarch64, arm, x86_64)

CARCH=$1 makepkg -f

rm -rf ./src ./pkg

[ -d ../packages ] && mv *.zst ../packages/ || { mkdir ../packages; mv *.zst ../packages; }
