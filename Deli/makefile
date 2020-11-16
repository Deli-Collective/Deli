CONSTANTS_SOURCE = Constants.cs
CONTENTS         = $(addsuffix .dll,$(NAME) Atlas DotNetZip)
ZIP              = $(NAME).zip
TEMP             = temp
TEMP_PLUGINS     = $(TEMP)/BepInEx/plugins/Deli

.PHONY: all macros build pack clean

all: clean macros build pack

macros:
	tee $(CONSTANTS_SOURCE).old < $(CONSTANTS_SOURCE)
	sed -i \ 
		's/STUB_GIT_BRANCH/$(GIT_BRANCH/' \
		's/STUB_GIT_HASH/$(GIT_HASH)/' \
		$(CONSTANTS_SOURCE)

build:
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

pack: build
	mkdir -p $(TEMP_PLUGINS)
	
	mv $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS)) $(TEMP_PLUGINS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	if [ -f $(CONSTANTS_SOURCE).old ]; then \
		mv $(CONSTANTS_SOURCE).old $(CONSTANTS_SOURCE) \
	fi
	rm -rf $(TEMP)
	rm -f $(ZIP)