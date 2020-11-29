CONTENTS = manifest.json MonoMod.exe $(NAME).dll
ZIP      = $(NAME).zip

.PHONY: all build pack clean

all: clean build pack

build:
	dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

pack: build
	zip -9j $(ZIP) $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS))

clean:
	rm -f $(ZIP)
