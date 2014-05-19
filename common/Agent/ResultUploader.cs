using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace MonoCounters.Common.Agent
{
	public class ResultUploader
	{
		string Address { get; set; }

		public ResultUploader (string address)
		{
			Address = address;
		}

		public void Upload (int recipe_id, Stream output)
		{
			var boundary = "----------" + GenerateRandomString (10);
			var request = (HttpWebRequest)WebRequest.Create (String.Format ("http://{0}/run", Address));

			request.Method = "POST";
			request.ContentType = "multipart/form-data; boundary=" + boundary;
			request.Timeout = 10000;

			using (var stream = request.GetRequestStream ()) {
				foreach (var e in new Dictionary<string, object> { { "recipe_id", recipe_id }, { "output", output } }) {
					new MemoryStream (Encoding.ASCII.GetBytes (String.Format (
						"--{0}\r\n", boundary))).CopyTo (stream);

					if (e.Value is Stream) {
						new MemoryStream (Encoding.ASCII.GetBytes (String.Format (
							"Content-Disposition: form-data; name=\"{0}\"; filename=\"{0}\"\r\n", e.Key))).CopyTo (stream);
						new MemoryStream (Encoding.ASCII.GetBytes (
							"Content-Type: application/octet-stream\r\n\r\n")).CopyTo (stream);
						(e.Value as Stream).CopyTo (stream);
					} else {
						new MemoryStream (Encoding.ASCII.GetBytes (String.Format (
							"Content-Disposition: form-data; name=\"{0}\"\r\n", e.Key))).CopyTo (stream);
						new MemoryStream (Encoding.ASCII.GetBytes ("\r\n")).CopyTo (stream);
						new MemoryStream (Encoding.ASCII.GetBytes (e.Value.ToString ())).CopyTo (stream);
					}

					new MemoryStream (Encoding.ASCII.GetBytes ("\r\n")).CopyTo (stream);
				}

				new MemoryStream (Encoding.ASCII.GetBytes ("--" + boundary + "--")).CopyTo (stream);
			}

			using (var response = (HttpWebResponse)(request.GetResponse ())) {
				if (response.StatusCode != HttpStatusCode.OK)
					throw new Exception (String.Format ("Received {0} response code", response.StatusCode));
			}
		}

		private static string GenerateRandomString (int length)
		{
			var chars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray ();
			var builder = new StringBuilder ();
			var random = new Random ();

			for (int i = 0, max = chars.Length; i < length; i++)
				builder.Append (chars [random.Next (max)]);

			return builder.ToString ();
		}
	}
}

