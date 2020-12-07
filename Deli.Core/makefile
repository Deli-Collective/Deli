CONTENTS = $(NAME).Patcher.dll $(NAME).Runtime.dll

.PHONY: all macros-pre compile macros-post clean

all: clean $(PACKAGE) macros-post

macros-pre:
	tee $(MANIFEST).old < $(MANIFEST)
	sed -i \
		-e 's|MACRO_VERSION|$(VERSION)|g' \
		$(MANIFEST)

compile: macros-pre
	$(BUILD) Deli.Core.Patcher
	$(BUILD) Deli.Core.Runtime

$(PACKAGE): compile
	zip -9j $@ $(MANIFEST) $(addprefix Deli.Core.*/bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS))

macros-post: $(PACKAGE)
	mv $(MANIFEST_OLD) $(MANIFEST)

clean:
	rm -f $(PACKAGE)
