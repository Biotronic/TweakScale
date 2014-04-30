REFDIR = 'C:\Users\root\Software\KSP_win\KSP_Data\Managed'
REFS = Assembly-CSharp,UnityEngine
MCSFLAGS = -lib:$(REFDIR) -r:$(REFS) -target:library
MCS = mcs $(MCSFLAGS)

%.dll: %.cs
	$(MCS) $^

all: $(patsubst %.cs,%.dll,$(wildcard *.cs))

clean:
	rm *.dll
