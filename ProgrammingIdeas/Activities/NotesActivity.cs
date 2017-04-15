using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using ProgrammingIdeas.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProgrammingIdeas.Activities;
using System;
using ProgrammingIdeas.Adapters;

namespace ProgrammingIdeas.Activities
{
    [Activity(Label = "Notes", Theme = "@style/AppTheme")]
    public class NotesActivity : BaseActivity
    {
        private List<Note> notes;
        private RecyclerView recycler;
        private RecyclerView.LayoutManager manager;
        private NotesAdapter adapter;
        private ViewSwitcher switcher, notesActivitySwitcher;
        private readonly string notesdb = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "notesdb");
        private string noteText;
        private bool isNoteEditing = false;
		private View emptyState;

        public override int LayoutResource
        {
            get
            {
                return Resource.Layout.notesactivity;
            }
        }

        public override bool HomeAsUpEnabled
        {
            get
            {
                return true;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            notes = JsonConvert.DeserializeObject<List<Note>>(DBAssist.DeserializeDB(notesdb));
			notes = notes ?? new List<Note>();
            recycler = FindViewById<RecyclerView>(Resource.Id.notesRecyclerView);
			emptyState = FindViewById(Resource.Id.empty);
			if (notes.Count == 0)
				Adapter_OnAdapterEmpty(this, new EventArgs());
            manager = new LinearLayoutManager(this);
            adapter = new NotesAdapter(notes);
            adapter.EditClicked += Adapter_EditClicked;
            recycler.SetAdapter(adapter);
            recycler.SetLayoutManager(manager);
            recycler.SetItemAnimator(new DefaultItemAnimator());
        }

        private void Adapter_OnAdapterEmpty(object sender, System.EventArgs e)
        {
            recycler.Visibility = ViewStates.Gone;
			emptyState.Visibility = ViewStates.Visible;
			emptyState.FindViewById<TextView>(Resource.Id.infoText).Text += " notes.";
        }

        private void Adapter_EditClicked(object sender, int e)
        {
            var view = recycler.GetChildAt(e);
            var input = view.FindViewById<TextView>(Resource.Id.notesEdit);
            var editNote = view.FindViewById<TextView>(Resource.Id.notesEditBtn);
            var content = view.FindViewById<TextView>(Resource.Id.notesContent);

            if (isNoteEditing == true) //user wants to save note
            {
                noteText = input.Text;
                switcher.ShowPrevious();
                editNote.Text = noteText;
                isNoteEditing = false;
                editNote.Text = "Edit this note";
                var note = notes[e];
                var newNote = new Note() { Category = note.Category, Content = noteText.Length == 0 ? null : noteText, Title = note.Title };
                var foundNote = notes.FirstOrDefault(x => x.Title == note.Title);
                if (foundNote == null) // existing note wasn't found
                {
                    if (newNote.Content != null)
                        notes.Add(newNote);
                    else
                        editNote.Text = "You have no notes for this idea. Tap the button below to add one.";
                }
                else //existing note was found
                {
                    if (newNote.Content == null)
                    {
                        editNote.Text = "You have no notes for this idea. Tap the button below to add one.";
                        notes.Remove(foundNote);
                        adapter.NotifyItemRemoved(e);
                    }
                    else
                    {
                        notes.Remove(foundNote);
                        notes.Add(newNote);
                    }
                }
                content.Text = noteText;
            }
            else
            { //user wants to edit note
                if (content.Text.Contains("You have no notes for this idea"))
                    input.Text = "";
                else
                    input.Text = content.Text;
                input.RequestFocus();
                isNoteEditing = true;
                switcher.ShowNext();
                editNote.Text = "Save this note";
            }
            if (notes.Count == 0)
                Adapter_OnAdapterEmpty(sender, new EventArgs());
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    NavigateAway();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            NavigateAway();
            base.OnBackPressed();
        }

        private void NavigateAway()
        {
            var intent = new Intent(this, typeof(CategoryActivity));
            NavigateUpTo(intent);
            OverridePendingTransition(Resource.Animation.push_up_in, Resource.Animation.push_up_out);
        }

        protected override void OnStop()
        {
            DBAssist.SerializeDB(notesdb, notes);
            base.OnStop();
        }
    }
}