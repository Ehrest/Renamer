using System;
using System.Collections;
using System.IO;
using System.Net;

using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;


namespace Ehrest.Editor.Renamer
{
    public class VersioningTool
    {
        public readonly string PackageName;

        public bool IsReady => _packageInfo != null && _versions.Length > 0;
        public string[] Versions => _versions;
        public string CurrentVersion => _packageInfo.version;

        private ListRequest _request;
        private UnityEditor.PackageManager.PackageInfo _packageInfo;
        private string _repoPath;
        private string RepoPathGitExtension => _repoPath + ".git";
        private string[] _versions;

        private readonly VersioningWidget _widget;
        private static bool _refreshFlag;

        public VersioningTool(string packageName)
        {
            PackageName = packageName;

            _widget = new VersioningWidget();

            GetPackageInfo();
        }

        public VersioningTool(string packageName, VersioningWidget widget)
        {
            PackageName = packageName;

            _widget = widget;

            GetPackageInfo();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            _refreshFlag = true;
        }

        public void Refresh()
        {
            _refreshFlag = false;
            _packageInfo = null;
            _versions = new string[0];

            GetPackageInfo();
        }

        public bool IsUpdateAvailable()
        {
            return !_versions[0].Equals(CurrentVersion);
        }

        public void ChangeVersionTo(string version)
        {
            UnityEditor.PackageManager.Client.Add(GetReleaseURL(RepoPathGitExtension, version));
        }

        public void DrawWidget()
        {
            if(_refreshFlag)
                Refresh();

            _widget.Draw(this);
        }

        private string GetReleaseURL(string repo, string version)
        {
            return $@"https://github.com/{repo}#{version}";
        }
 
 #region PackageFetch

        private void GetPackageInfo()
        {
            _request = UnityEditor.PackageManager.Client.List();

            StartWaitingForRequest();
        }

        private void WaitForRequest()
        {
            if (_request == null)
            {
                StopWaitingForRequest();
                return;
            }

            if (_request.IsCompleted)
            {
                OnListReceived();
            }
        }

        private void OnListReceived()
        {
            StopWaitingForRequest();

            if (_request.Status != UnityEditor.PackageManager.StatusCode.Failure)
            {
                foreach (var packageInfo in _request.Result)
                {
                    if (packageInfo.name.Equals(PackageName))
                        OnPackageFound(packageInfo);
                }
            }
        }

        private void StartWaitingForRequest()
        {
            EditorApplication.update += WaitForRequest;
        }

        private void StopWaitingForRequest()
        {
            EditorApplication.update -= WaitForRequest;
        }

        private void OnPackageFound(UnityEditor.PackageManager.PackageInfo packageInfo)
        {
            _packageInfo = packageInfo;

            #if UNITY_2020_1_OR_NEWER

            string split = "github.com/";
            string gitExt = ".git";

            _repoPath = packageInfo.repository.url.Substring(packageInfo.repository.url.IndexOf(split) + split.Length);
            _repoPath = _repoPath.Substring(0, _repoPath.Length - gitExt.Length);
            #else
            _repoPath = RenamerSettings.Repository;
            #endif

            InitializeVersions(_repoPath);
        }

 #endregion PackageFetch

 #region VersionFetch
    
        private void InitializeVersions(string repo)
        {
            string URL = $@"https://api.github.com/repos/{repo}/releases";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.UserAgent = "PHP";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            EditorGUIUtility.systemCopyBuffer = TransformJsonArrayToObject(jsonResponse, nameof(GithubReleases.All));
            GithubReleases releases = JsonUtility.FromJson<GithubReleases>(TransformJsonArrayToObject(jsonResponse, nameof(GithubReleases.All)));

            _versions = new string[releases.All.Length];
            for (int i = 0; i < releases.All.Length; i++)
            {
                _versions[i] = releases.All[i].tag_name;
            }
        }

        private string TransformJsonArrayToObject(string json, string arrayName)
        {
            return $"{{\"{arrayName}\":{json}}}";
        }

 #endregion VersionFetch
    }
}