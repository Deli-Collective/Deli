include makefile.var

# Local
NAME                 = Deli
ZIP                  = $(NAME).zip
TEMP                 = temp

PROJ_PATCHER         = $(NAME).Patcher
PROJ_SETUP           = $(NAME).Setup
PROJS                = $(PROJ_PATCHER) $(PROJ_SETUP)

TEMP_PATCHERS        = $(TEMP)/BepInEx/patchers/Deli
TEMP_PLUGINS         = $(TEMP)/BepInEx/plugins/Deli
TEMP_DIRS            = $(TEMP_PATCHERS) $(TEMP_PLUGINS)

CONTENTS_PATCHERS    = $(addsuffix .dll,$(PROJ_PATCHER) DotNetZip Newtonsoft.Json)
CONTENTS_PLUGINS     = $(addsuffix .dll,$(PROJ_SETUP))

.PHONY: FORCE all clean

all: clean $(ZIP)
FORCE:

$(PROJS): FORCE
	"$(MAKE)" -C "$@" NAME="$@" PACKAGE="$@.deli"

$(ZIP): $(PROJS)
	for d in $(TEMP_DIRS); do \
		mkdir -p $$d; \
	done
	
	mv $(addprefix $(PROJ_PATCHER)/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS_PATCHERS)) $(TEMP_PATCHERS)/
	mv $(addprefix $(PROJ_SETUP)/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS_PLUGINS)) $(TEMP_PLUGINS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)
