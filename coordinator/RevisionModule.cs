using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Nancy;
using Nancy.Responses;

using Newtonsoft.Json;

using MonoCounters.Models;

namespace MonoCounters.Coordinator
{
	public class RevisionModule : NancyModule
	{
		public static string Root = Path.Combine (Directory.GetCurrentDirectory (), "data");

		public RevisionModule ()
		{
			Get ["/revisions/{project}/{architecture}"] = parameters => {
				List<Revision> revisions = Revision.FindByProjectAndArchitecture (parameters.project, parameters.architecture);

				if (revisions.Count == 0)
					return new TextResponse (HttpStatusCode.NotFound, "Architecture not found");

				var dirname = String.Format ("{0}/{1}/{2}", Root, parameters.project, parameters.architecture);

				if (!Directory.Exists (dirname))
					return new TextResponse (HttpStatusCode.NotFound, "Directory not found");

				IEnumerable<string> files = Directory.EnumerateFiles (dirname, "*.tar.gz", SearchOption.TopDirectoryOnly);

				return new TextResponse (HttpStatusCode.OK, JsonConvert.SerializeObject (
						revisions.Where (r => files.Any (f => f.Contains (r.Commit)))));
			};

			Get ["/revision/{project}/{architecture}/{commit}.tar.gz"] = parameters => {
				var filename = String.Format ("{0}/{1}/{2}/{3}.tar.gz", Root, parameters.project, parameters.architecture, parameters.commit);

				if (!File.Exists (filename)) {
					return new TextResponse (HttpStatusCode.NotFound, "Commit not found");
				}

				return new Response () {
					StatusCode = HttpStatusCode.OK,
					ContentType = "application/octet-stream",
					Contents = (s) => {
						using (var file = new FileStream (filename, FileMode.Open, FileAccess.Read)) {
							file.CopyTo (s);
						}
					}
				};
			};

			Post ["/revision/{project}/{architecture}/{commit}.tar.gz"] = parameters => {
				var dirname = String.Format ("{0}/{1}/{2}", Root, parameters.project, parameters.architecture);

				if (!Directory.Exists (dirname))
					Directory.CreateDirectory (dirname);

				using (var body = Request.Body) {
					using (var file = new FileStream (String.Format ("{0}/{1}.tar.gz", dirname, parameters.commit), FileMode.Create, FileAccess.Write)) {
						body.CopyTo (file);
					}
				}

				var revision = Revision.FindByProjectArchitectureAndCommit (parameters.project, parameters.architecture, parameters.commit) ??
						new Models.Revision () { Project = parameters.project, Architecture = parameters.architecture,
							Commit = parameters.commit, CreationDate = DateTime.Now }.Save ();

				return new TextResponse (HttpStatusCode.OK, JsonConvert.SerializeObject (revision));
			};

			OnError += (ctx, e) => {
				return new TextResponse (HttpStatusCode.InternalServerError, e.ToString ());
			};
		}
	}
}

