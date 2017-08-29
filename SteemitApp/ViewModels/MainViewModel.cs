using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using System.Linq;
using MvvmCross.Core.Navigation;
using MvvmCross.Platform;

namespace SteemitApp.Core.ViewModels
{
    public class MainViewModel : MvxViewModel
    {
        private readonly IRepository repository;
        private readonly IMvxNavigationService navigation;
        private DiscussionCategory category = DiscussionCategory.Trending;

        public MainViewModel(IRepository Repository, IMvxNavigationService Navigation)
        {
            repository = Repository;
            navigation = Navigation;
        }
        
        public override async Task Initialize()
        {
            await loadDiscussions();
        }

        private async Task loadDiscussions() 
        {
            var payload = new DiscussionPayload("steem", "10");
            payload.Type = category;

            var result = await repository.LoadDiscussions(payload);
            if (result.StatusCode == System.Net.HttpStatusCode.OK) 
            {
                foreach (var discussion in result.Data)
                {
                    Discussions.Add(discussion);
                }
            }
        }

        public MvxObservableCollection<PostPresentation> Discussions { get; set; } = new MvxObservableCollection<PostPresentation>();

        public IMvxCommand LoadMoreCommand => new MvxCommand(LoadMore);
        private async void LoadMore() 
        {
            var lastEntry = Discussions.LastOrDefault();
            if (lastEntry != null) 
            {
                var payload = new DiscussionPayload("steem", "10", lastEntry.Author, lastEntry.Permlink);
                payload.Type = category;

                var result = await repository.LoadDiscussions(payload);
                if (result.StatusCode == System.Net.HttpStatusCode.OK) 
                {
                    for (int i = 1; i < result.Data.Count; i++)
                    {
                        Discussions.Add(result.Data[i]);
                    }
                }
            }
        }

        public IMvxCommand SelectTableItemCommand => new MvxCommand<PostPresentation>(SelectTableItem);
        private void SelectTableItem(PostPresentation Post) 
        {
            Mvx.RegisterSingleton<PostPresentation>(Post);
            navigation.Navigate<DetailViewModel>();
        }

        public IMvxCommand SegmentChangedCommand => new MvxCommand<int>(SegmentChanged);
        private async void SegmentChanged(int index) 
        {
            category = (DiscussionCategory)index;
            Discussions.Clear();
            await loadDiscussions();
        }
    }
}