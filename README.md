# ReSharperTutorials
* To build the package use `nuget pack pluginname.nuspec`
* Install R#'s required wave into a separate hive (say, Test)
* Install the plugin into this hive using R#'s extension manager
* To automate the plugin run and debug:
  * Add to project's .csproj file 
<pre>
		  &lt;PropertyGroup&gt;
		    &lt;HostFullIdentifier&gt;ReSharperPlatformVs14Test&lt;/HostFullIdentifier&gt;
		  &lt;/PropertyGroup&gt;
</pre>
This will tell msbuild to copy the built plugin dll to the required VS hive with targeted R# (SDK adds a special CopyBuild step and this property is its argument).  
Here `Vs14` stands for VS version (2015), `Test` stands for the hive.
  * In VS, in project's properties in **Debug**, specify **Start external program** to start VS
say `C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe`
and **Command line arguments** - `/rootSuffix Test`
this will run plugin in a required VS instance after the build.

IMPORTANT: if a plugin contains some integration points with VS (e.g., Actions, Menus...), and your build updates these points, the plugin MUST be reinstalled via R# Extension Manager!!!
