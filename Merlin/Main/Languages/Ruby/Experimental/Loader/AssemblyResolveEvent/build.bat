csc /target:library a3.cs
csc /target:library a2.cs
csc /target:library /r:a2.dll /r:a3.dll a1.cs
