export SHELL            = /bin/bash

# Settings
export CONFIG          ?= Release
export FRAMEWORK       ?= net35
export VERSION         ?= 0.0.0
       NUGET           ?= nuget
export NUGET_DIR        = ../$(NUGET)

# DLL metadata
export GIT_DESCRIBE     = $(shell git describe --long --always --dirty)
export GIT_BRANCH       = $(shell git rev-parse --abbrev-ref HEAD)
export GIT_HASH         = $(shell git rev-parse HEAD)
export BUILD_PROPERTIES = /p:Version="$(VERSION)" /p:RepositoryBranch="$(GIT_BRANCH)" /p:RepositoryCommit="$(GIT_HASH)"

# Local
NAME              = Deli
ZIP               = $(NAME).zip
TEMP              = temp

PROJ_PATCHER      = $(NAME)
PROJ_RUNTIME      = $(NAME).Runtime
PROJ_CORE         = $(NAME).Core
PROJ_MONOMOD      = $(NAME).MonoMod

PROJS_FRAMEWORK   = $(PROJ_PATCHER) $(PROJ_RUNTIME)
PROJS_LIBS        = $(PROJ_CORE) $(PROJ_MONOMOD)
PROJS             = $(PROJS_FRAMEWORK) $(PROJS_LIBS)

TEMP_PATCHERS     = $(TEMP)/BepInEx/patchers/Deli
TEMP_PLUGINS      = $(TEMP)/BepInEx/plugins/Deli
TEMP_MODS         = $(TEMP)/mods
TEMP_DIRS         = $(TEMP_PATCHERS) $(TEMP_PLUGINS) $(TEMP_MODS)

CONTENTS_PATCHERS = $(addsuffix .dll,$(PROJ_PATCHER) ADepIn DotNetZip)
CONTENTS_PLUGINS  = $(addsuffix .dll,$(PROJ_RUNTIME))
CONTENTS_MODS     = $(addsuffix /*.zip,$(PROJS_LIBS))

.PHONY: all clean nested-all

all: clean nested-all $(ZIP)

nested-all: $(PROJS)
	for p in $^; do \
		$(MAKE) -C $$p NAME=$$p; \
	done

$(ZIP): nested-all
	for d in $(TEMP_DIRS); do \
		mkdir -p $$d; \
	done
	
	mv $(addprefix $(PROJ_PATCHER)/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS_PATCHERS)) $(TEMP_PATCHERS)/
	mv $(addprefix $(PROJ_RUNTIME)/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS_PLUGINS)) $(TEMP_PLUGINS)/
	mv $(CONTENTS_MODS) $(TEMP_MODS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)
