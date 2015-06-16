using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver;
using M101DotNet.WebApp.Models;
using M101DotNet.WebApp.Models.Home;
using MongoDB.Bson;
using System.Linq.Expressions;

namespace M101DotNet.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var blogContext = new BlogContext();
            // XXX WORK HERE
            // find the most recent 10 posts and order them
            // from newest to oldest

            List<Post> recentPosts = await blogContext.Posts.Find(new BsonDocument())
                .Sort(Builders<Post>.Sort.Descending(x => x.CreatedAtUtc))
                .Limit(10)
                .ToListAsync();

            var model = new IndexModel
            {
                RecentPosts = recentPosts
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult NewPost()
        {
            return View(new NewPostModel());
        }

        [HttpPost]
        public async Task<ActionResult> NewPost(NewPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // Insert the post into the posts collection
            var post = new Post(model.Title, model.Content, model.Tags, User.Identity.Name);

            await blogContext.Posts.InsertOneAsync(post);

            return RedirectToAction("Post", new { id = post.Id });
        }

        [HttpGet]
        public async Task<ActionResult> Post(string id)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find the post with the given identifier

            var post = await blogContext.Posts.Find(x => x.Id == id).SingleOrDefaultAsync();

            if (post == null)
            {
                return RedirectToAction("Index");
            }

            var model = new PostModel
            {
                Post = post
            };

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> Posts(string tag = null)
        {
            var blogContext = new BlogContext();

            // XXX WORK HERE
            // Find all the posts with the given tag if it exists.
            // Otherwise, return all the posts.
            // Each of these results should be in descending order.
            var postsViewModel = new List<Post>();

            if (tag == null)
            {
                var allPosts = await blogContext.Posts.Find(new BsonDocument()).ToListAsync();

                foreach (var post in allPosts.OrderByDescending(x => x.CreatedAtUtc))
                {
                    postsViewModel.Add(post);
               
                }
            }
            else
            {
                var tags = tag.Split(',').ToList();

                var allPosts = await blogContext.Posts.Find(new BsonDocument()).ToListAsync();

                foreach (var post in allPosts.Where(x => x.Tags.Intersect(tags).Any()).OrderByDescending(x => x.CreatedAtUtc))
                {
                    postsViewModel.Add(post);
                }
            }

            return View(postsViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> NewComment(NewCommentModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Post", new { id = model.PostId });
            }

            var blogContext = new BlogContext();
            // XXX WORK HERE
            // add a comment to the post identified by model.PostId.
            // you can get the author from "this.User.Identity.Name"

            var comment = new Comment
            {
                Author = User.Identity.Name,
                Content = model.Content,
                CreatedAtUtc = DateTime.Now
            };

            await blogContext.Posts.FindOneAndUpdateAsync(x => x.Id == model.PostId,
                Builders<Post>.Update.AddToSet(x => x.Comments, comment));

            return RedirectToAction("Post", new { id = model.PostId });
        }
    }
}