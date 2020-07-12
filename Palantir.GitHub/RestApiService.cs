namespace Palantir.GitHub
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using Palantir.Domain.Configurations;
    using Palantir.Domain.Models;
    using Palantir.Domain.Services;

    public class RestApiService : IMergeRequestService
    {
        private readonly HttpClient client;

        private readonly List<string> restApiUrls;

        private readonly RestApiHeader restApiHeader;

        public RestApiService(List<string> urls, RestApiHeader header)
        {
            this.client = new HttpClient();
            this.restApiUrls = urls;
            this.restApiHeader = header;
        }

        public List<MergeRequest> GetMergeRequests()
        {
            MergeRequestAdapter adapter = new MergeRequestAdapter();
            List<MergeRequest> result = new List<MergeRequest>();

            this.client.DefaultRequestHeaders.Add("Authorization", $"token {this.restApiHeader.AuthorizationToken}");
            this.client.DefaultRequestHeaders.Add("User-Agent", this.restApiHeader.UserAgent);

            foreach (string item in this.restApiUrls)
            {
                HttpResponseMessage data = this.client.GetAsync(item).Result;

                if (data.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Error al realizar petición a {item}");
                }

                string dataContent = data.Content.ReadAsStringAsync().Result;
                IEnumerable<GitHubPullRequest> gitHubPullRequests = JsonConvert.DeserializeObject<IEnumerable<GitHubPullRequest>>(dataContent);

                foreach (GitHubPullRequest gitHubPullRequest in gitHubPullRequests)
                {
                    result.Add(adapter.GetMergeRequest(gitHubPullRequest));
                }
            }

            return result;
        }
    }
}