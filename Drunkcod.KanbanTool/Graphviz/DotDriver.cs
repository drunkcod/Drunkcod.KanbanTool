using System;
using System.Diagnostics;

namespace Drunkcod.Graphviz
{
	public class DotDriver
	{
		readonly string dotPath;

		public DotDriver(string dotPath) { this.dotPath = dotPath; }

		public string Svg(string input) {
			var dot = Process.Start(new ProcessStartInfo {
				FileName = dotPath,
				Arguments = "-Tsvg",
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			});
			dot.StandardInput.WriteLine(input);
			dot.StandardInput.Close();
			var r = dot.StandardOutput.ReadToEnd();
			dot.WaitForExit();
			if(dot.ExitCode != 0)
				throw new InvalidOperationException(dot.StandardError.ReadToEnd());
			return r;
		}
	}
}
