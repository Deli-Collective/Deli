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

CONTENTS_PATCHERS_DLLS = $(addsuffix .dll,$(PROJ_PATCHER) DotNetZip Deli.Newtonsoft.Json)
CONTENTS_PATCHERS_EXES = $(addsuffix .exe, MonoMod MonoMod.RuntimeDetour.HookGen)
CONTENTS_PLUGINS       = $(addsuffix .dll,$(PROJ_SETUP))

.PHONY: build all clean

all: clean $(ZIP)

build:
	for p in $(PROJS); do \
		"$(MAKE)" -C "$$p" precompile NAME="$$p" || true; \
	done
	$(BUILD)
	for p in $(PROJS); do \
		"$(MAKE)" -C "$$p" postcompile NAME="$$p" || true; \
	done

$(ZIP): build
	for p in $(PROJS); do \
		"$(MAKE)" -C "$$p" pack NAME="$$p" || true; \
	done
	
	for d in $(TEMP_DIRS); do \
		mkdir -p $$d; \
	done
	
	mv $(addprefix $(PROJ_PATCHER)/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS_PATCHERS_DLLS)) "$(TEMP_PATCHERS)/"
	for f in $(CONTENTS_PATCHERS_EXES); do \
		mv -- "$(PROJ_PATCHER)/bin/$(CONFIG)/$(FRAMEWORK)/$$f" "$(TEMP_PATCHERS)/$${f%.exe}.dll"; \
	done
	mv $(addprefix $(PROJ_SETUP)/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS_PLUGINS)) "$(TEMP_PLUGINS)/"

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)
