using Android.App;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Util;
using Helpers;
using Newtonsoft.Json;
using ProgrammingIdeas.Activities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.Support.V7.Widget;

namespace ProgrammingIdeas.Helpers
{
    internal static class CloudDB
    {
        private static string oldDBPath = Path.Combine(Global.APP_PATH, "ideasdb");
        private static string newideastxtPath = Path.Combine(Global.APP_PATH, "newideastxt");
        private static List<Category> newDB;
        private static string newideastxt;
        private static HttpClient client = new HttpClient();
        private static Activity activity;
        private const string TAG = "ALANSADEBUG";

        public static async Task Startup(Action retryMethod, Snackbar snack)
        {
            try
            {
				activity = App.CurrentActivity;
                if (!File.Exists(oldDBPath)) // First launch, as in just installed app and launched it
                {
					var source = new CancellationTokenSource();
					source.CancelAfter(12000);
                    var response = await client.GetAsync(AppResources.DbLink, source.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        var payload = await response.Content.ReadAsStringAsync();
                        newDB = JsonConvert.DeserializeObject<List<Category>>(payload);
                        var newideasdbResponse = await client.GetAsync(AppResources.NewIdeasDbLink);
                        if (newideasdbResponse.IsSuccessStatusCode)
                        {
                            newideastxt = await newideasdbResponse.Content.ReadAsStringAsync();
                            Global.Categories = newDB;
                            DBAssist.SerializeDB(oldDBPath, Global.Categories);
                            DBAssist.SerializeDB(newideastxtPath, newideastxt);
                        }
                    }
                }
                else // Not first launch, checking if new ideas available and invalidating old ideas if true
                    StartLowkeyInvalidation();

                // To prevent too many requests to server, only invalidate cache if app is opening fresh from launcher
                Global.LockRequests = true;
            }
            catch (TaskCanceledException)
            {
                snack.SetText("Can't load ideas. Your connection might be too slow.").SetAction("Retry", (v) => retryMethod?.Invoke()).Show();
            }
            catch (HttpRequestException)
            {
                snack.SetText("Can't load ideas. Check your connection.").SetAction("Retry", (v) => retryMethod?.Invoke()).Show();
            }
            catch (Exception e)
            {
                snack.SetText($"Oops! {e.Message}.").SetAction("Retry", (v) => retryMethod?.Invoke()).Show();
            }
        }

        private static void Notify(int newIdeasCount)
        {
			activity.RunOnUiThread(() =>
			{
				string notifContent = newIdeasCount == 1 ? $"1 new idea is available." : $"{newIdeasCount.ToString()} new ideas are available.";
				var intent = new Intent(activity, typeof(CategoryActivity));
				intent.PutExtra("NewIdeasNotif", true);
				var pendingIntent = PendingIntent.GetActivity(activity, 1960, intent, PendingIntentFlags.UpdateCurrent);

				var notif = new NotificationCompat.Builder(activity)
												  .SetContentTitle("New ideas available.")
												  .SetContentText(notifContent)
												  .SetContentIntent(pendingIntent)
												  .SetSmallIcon(Resource.Mipmap.notif_icon)
												  .Build();
				var notifManager = (NotificationManager)activity.GetSystemService(Context.NotificationService);
				notifManager.Notify(1957, notif);
			});
        }

        private static bool NewIdeasAvailable(List<Category> oldIdeas, List<Category> newIdeas)
        {
            for (int i = 0; i < oldIdeas.Count; i++)
            {
                if (newIdeas[i].CategoryCount > oldIdeas[i].CategoryCount)
                    return true;
            }
            return false;
        }

        private static async void StartLowkeyInvalidation()
        {
            try
            {
                if (!Global.LockRequests)
                {
                    Log.Debug(TAG, "Starting lowkey invalidation");
                    Global.Categories = DBAssist.GetDB(oldDBPath);
                    var response = await client.GetAsync(AppResources.DbLink);
                    if (response.IsSuccessStatusCode)
                    {
                        var payload = await response.Content.ReadAsStringAsync();
                        newDB = JsonConvert.DeserializeObject<List<Category>>(payload);
                        var newideasdbResponse = await client.GetAsync(AppResources.NewIdeasDbLink);
                        if (newideasdbResponse.IsSuccessStatusCode)
                        {
                            newideastxt = await newideasdbResponse.Content.ReadAsStringAsync();

                            if (NewIdeasAvailable(Global.Categories, newDB))
                            {
                                Log.Debug(TAG, "New ideas available");
                                var newIdeasContent = newideastxt.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                Notify(newIdeasContent.Length);
                            }

                            Log.Debug(TAG, "Starting full invalidation");

                            var notesPath = Path.Combine(Global.APP_PATH, "notesdb");
							if(!File.Exists(notesPath))
								File.Create(notesPath);
                            var notes = JsonConvert.DeserializeObject<List<Note>>(DBAssist.DeserializeDB(notesPath));
                            notes = notes ?? new List<Note>();
							for (int i = 0; i < newDB.Count; i++)
                            {
                                for (int j = 0; j < newDB[i].Items.Count; j++)
                                {
                                    var newItem = newDB[i].Items[j];
                                    var oldItem = Global.Categories[i].Items.FirstOrDefault(x => x.Id == newItem.Id);
                                    Note note = null;
									if (oldItem != null)
									{
										note = notes.FirstOrDefault(x => x.Title == oldItem.Title);
										if(note != null)
											Log.Debug(TAG, $"Note *{note.Title}*, found for old idea *{oldItem.Title}* placed at new idea *{newItem.Title}*");
									}

                                    newItem.Note = note;
                                    newItem.State = oldItem?.State;
                                }
                            }

							// Do not delete bookmarks for this update. Seems very important to people.
                            /*if (PreferenceHelper.GetBoolean("bookmarksDeleted", false) == false)
                            {
                                var path = Path.Combine(Global.APP_PATH, "bookmarks.json");
                                if (File.Exists(path))
                                    File.Delete(path);
                                PreferenceHelper.PutBoolean("bookmarksDeleted", true);
                            }*/

                            DBAssist.SerializeDB(oldDBPath, newDB);
                            DBAssist.SerializeDB(newideastxtPath, newideastxt);
                            Global.Categories = newDB;

                            Log.Debug(TAG, "Invalidation completed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug(TAG, e.Message + e.Source + e.StackTrace);
            }
        }
    }
}