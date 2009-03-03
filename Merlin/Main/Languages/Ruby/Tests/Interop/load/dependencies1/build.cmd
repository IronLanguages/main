tf edit A\A.dll B\B.dll
csc /target:library /out:B\B.dll B.cs
csc /target:library /out:A\A.dll /r:B\B.dll A.cs