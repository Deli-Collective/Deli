export SHELL            = /bin/bash

# Settings
export CONFIG          ?= Release
export FRAMEWORK       ?= net35

# DLL metadata
export GIT_DESCRIBE     = $(shell git describe --long --always --dirty)
export GIT_BRANCH       = $(shell git rev-parse --abbrev-ref HEAD)
export GIT_HASH         = $(shell git rev-parse HEAD)
export GIT_DATE         = $(shell git log -1 --format='%at')
export VERSION          = $(shell date --utc -d @"$(GIT_DATE)" +'%-y.%-m.%-d.%-H%M')
export BUILD_PROPERTIES = /p:RepositoryBranch="$(GIT_BRANCH)" /p:RepositoryCommit="$(GIT_HASH)"

# Local
PROJECTS = Deli Deli.Core

.PHONY: all

all: $(PROJECTS)
	for p in $^; do \
		$(MAKE) -C $$p NAME=$$p; \
	done
