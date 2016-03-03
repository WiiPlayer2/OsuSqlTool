using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;

namespace GitHubUpdate
{
    public static class Update
    {
        private const string GITHUB_URL = "https://github.com/{0}/{1}/releases/latest";

        private static bool init = false;
        private static string user;
        private static string rep;

        static Update()
        {
            CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            BuildDate = new DateTime(2000, 1, 1)
                .AddDays(CurrentVersion.Build)
                .AddSeconds(CurrentVersion.Revision * 2);

            if (CurrentVersion.Build == 0
                && CurrentVersion.Revision == 0)
            {
                throw new InvalidProgramException("AssemblyVersion is not set up.");
            }
        }

        public static void Init(string username, string repository)
        {
            if (init)
            {
                throw new InvalidOperationException("Already initialized.");
            }
            user = username;
            rep = repository;
            init = true;
        }

        private static void CheckInit()
        {
            if (!init)
            {
                throw new InvalidOperationException("Must be initialized.");
            }
        }

        public static bool HasUpdate()
        {
            CheckInit();
            try
            {
                var request = WebRequest.CreateHttp(ReleaseUrl);
                var response = request.GetResponse();
                var url = response.ResponseUri.AbsolutePath;
                var newVerStr = url.Substring(url.LastIndexOf('/') + 2);

                NewestVersion = new Version(newVerStr);

                return CurrentVersion < NewestVersion;
            }
            catch
            {
                return false;
            }
        }

        public static string ReleaseUrl
        {
            get
            {
                CheckInit();
                return string.Format(GITHUB_URL, user, rep);
            }
        }

        public static Version NewestVersion
        {
            get; private set;
        }

        public static Version CurrentVersion
        {
            get; private set;
        }

        public static DateTime BuildDate
        {
            get; private set;
        }
    }
}
