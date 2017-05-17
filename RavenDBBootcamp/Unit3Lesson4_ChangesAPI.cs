using System;
using System.Reactive.Linq;
using System.Windows.Forms;
using Raven.Abstractions.Data;
using Raven.Client;

namespace RavenDBBootcamp
{
	public class Unit3Lesson4_ChangesAPI
	{
		public static void Run()
		{
			//FirstExample();
			SecondFormsExample();
		}

		private static void FirstExample()
		{
			var subscription = Utility.DocumentStoreHolder.Store
				.Changes()
				.ForAllDocuments()
				.Subscribe(change =>
					Console.WriteLine($"{change.Type} on document {change.Id}"));

			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();

			subscription.Dispose();
		}

		private static void SecondFormsExample()
		{
			Application.Run(new EditCategoryForm());
		}

		public partial class EditCategoryForm : Form
		{
			public EditCategoryForm()
			{
				InitializeComponent();
			}

			private IDocumentSession _session;
			private Category _category;
			private IDisposable _subscription;
			private Etag _localEtag;

			private void LoadAndSubscribeButton_Click(object sender, EventArgs e)
			{
				_session = Utility.DocumentStoreHolder.Store.OpenSession();
				_category = _session.Load<Category>(CategoryIdTextbox.Text);

				if (_category == null)
				{
					MessageBox.Show("Category not found!");
					_session.Dispose();
				}
				else
				{
					NameTextbox.Text = _category.Name;
					DescriptionTextbox.Text = _category.Description;

					_localEtag = _session.Advanced.GetEtagFor(_category);
					_subscription = Utility.DocumentStoreHolder.Store
						.Changes()
						.ForDocument(CategoryIdTextbox.Text)
						.Where(DocumentChangedByOtherUser)
						.Subscribe(DocumentChangedOnServer);

					ToggleEditing();
				}
			}

			private bool DocumentChangedByOtherUser(DocumentChangeNotification change)
			{
				if (_savesCount == 0) return true;
				if (change.Etag.Restarts != _localEtag.Restarts) return true;

				var numberOfServerChanges = change.Etag.Changes - _localEtag.Changes;
				return (numberOfServerChanges > _savesCount);
			}

			private void DocumentChangedOnServer(DocumentChangeNotification change)
			{
				var shouldRefresh = MessageBox.Show(
											  "Document was changed on the server. Would like to refresh?",
											  "Alert", MessageBoxButtons.YesNo
										  ) == DialogResult.Yes;

				if (shouldRefresh)
				{
					_session.Advanced.Refresh(_category);
					_savesCount = 0;
					_localEtag = _session.Advanced.GetEtagFor(_category);
					this.Invoke((MethodInvoker)delegate
					{
						NameTextbox.Text = _category.Name;
						DescriptionTextbox.Text = _category.Description;
					});
				}
			}

			private void ToggleEditing()
			{
				var isEditing = NameTextbox.Enabled;

				// toggle Load and Subscribe feature
				CategoryIdTextbox.Enabled = isEditing;
				CategoryIdLabel.Enabled = isEditing;
				LoadAndSubscribeButton.Enabled = isEditing;

				// toggle Edit feature
				NameTextbox.Enabled = !isEditing;
				NameLabel.Enabled = !isEditing;
				DescriptionTextbox.Enabled = !isEditing;
				DescriptionLabel.Enabled = !isEditing;
				SaveButton.Enabled = !isEditing;
				UnsubscribeButton.Enabled = !isEditing;
			}

			private int _savesCount = 0;
			private void SaveButton_Click(object sender, EventArgs e)
			{
				_category.Name = NameTextbox.Text;
				_category.Description = DescriptionTextbox.Text;
				_savesCount++;
				_session.SaveChanges();
			}

			private void UnsubscribeButton_Click(object sender, EventArgs e)
			{
				_session.Dispose();
				_subscription.Dispose();
				ToggleEditing();
			}
		}

		partial class EditCategoryForm
		{
			/// <summary>
			/// Required designer variable.
			/// </summary>
			private System.ComponentModel.IContainer components = null;

			/// <summary>
			/// Clean up any resources being used.
			/// </summary>
			/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
			protected override void Dispose(bool disposing)
			{
				if (disposing && (components != null))
				{
					components.Dispose();
				}
				base.Dispose(disposing);
			}

			#region Windows Form Designer generated code

			/// <summary>
			/// Required method for Designer support - do not modify
			/// the contents of this method with the code editor.
			/// </summary>
			private void InitializeComponent()
			{
				this.CategoryIdTextbox = new System.Windows.Forms.TextBox();
				this.CategoryIdLabel = new System.Windows.Forms.Label();
				this.LoadAndSubscribeButton = new System.Windows.Forms.Button();
				this.NameLabel = new System.Windows.Forms.Label();
				this.NameTextbox = new System.Windows.Forms.TextBox();
				this.DescriptionLabel = new System.Windows.Forms.Label();
				this.DescriptionTextbox = new System.Windows.Forms.TextBox();
				this.SaveButton = new System.Windows.Forms.Button();
				this.UnsubscribeButton = new System.Windows.Forms.Button();
				this.SuspendLayout();
				// 
				// CategoryIdTextbox
				// 
				this.CategoryIdTextbox.Location = new System.Drawing.Point(96, 12);
				this.CategoryIdTextbox.Name = "CategoryIdTextbox";
				this.CategoryIdTextbox.Size = new System.Drawing.Size(271, 22);
				this.CategoryIdTextbox.TabIndex = 0;
				this.CategoryIdTextbox.Text = "categories/1";
				// 
				// CategoryIdLabel
				// 
				this.CategoryIdLabel.AutoSize = true;
				this.CategoryIdLabel.Location = new System.Drawing.Point(12, 15);
				this.CategoryIdLabel.Name = "CategoryIdLabel";
				this.CategoryIdLabel.Size = new System.Drawing.Size(78, 17);
				this.CategoryIdLabel.TabIndex = 1;
				this.CategoryIdLabel.Text = "CategoryID";
				// 
				// LoadAndSubscribeButton
				// 
				this.LoadAndSubscribeButton.Location = new System.Drawing.Point(436, 12);
				this.LoadAndSubscribeButton.Name = "LoadAndSubscribeButton";
				this.LoadAndSubscribeButton.Size = new System.Drawing.Size(174, 40);
				this.LoadAndSubscribeButton.TabIndex = 2;
				this.LoadAndSubscribeButton.Text = "Load and Subscribe";
				this.LoadAndSubscribeButton.UseVisualStyleBackColor = true;
				this.LoadAndSubscribeButton.Click += new System.EventHandler(this.LoadAndSubscribeButton_Click);
				// 
				// NameLabel
				// 
				this.NameLabel.AutoSize = true;
				this.NameLabel.Enabled = false;
				this.NameLabel.Location = new System.Drawing.Point(12, 98);
				this.NameLabel.Name = "NameLabel";
				this.NameLabel.Size = new System.Drawing.Size(45, 17);
				this.NameLabel.TabIndex = 4;
				this.NameLabel.Text = "Name";
				// 
				// NameTextbox
				// 
				this.NameTextbox.Enabled = false;
				this.NameTextbox.Location = new System.Drawing.Point(96, 95);
				this.NameTextbox.Name = "NameTextbox";
				this.NameTextbox.Size = new System.Drawing.Size(514, 22);
				this.NameTextbox.TabIndex = 3;
				// 
				// DescriptionLabel
				// 
				this.DescriptionLabel.AutoSize = true;
				this.DescriptionLabel.Enabled = false;
				this.DescriptionLabel.Location = new System.Drawing.Point(12, 126);
				this.DescriptionLabel.Name = "DescriptionLabel";
				this.DescriptionLabel.Size = new System.Drawing.Size(79, 17);
				this.DescriptionLabel.TabIndex = 6;
				this.DescriptionLabel.Text = "Description";
				// 
				// DescriptionTextbox
				// 
				this.DescriptionTextbox.Enabled = false;
				this.DescriptionTextbox.Location = new System.Drawing.Point(96, 123);
				this.DescriptionTextbox.Name = "DescriptionTextbox";
				this.DescriptionTextbox.Size = new System.Drawing.Size(514, 22);
				this.DescriptionTextbox.TabIndex = 5;
				// 
				// SaveButton
				// 
				this.SaveButton.Enabled = false;
				this.SaveButton.Location = new System.Drawing.Point(360, 151);
				this.SaveButton.Name = "SaveButton";
				this.SaveButton.Size = new System.Drawing.Size(120, 40);
				this.SaveButton.TabIndex = 7;
				this.SaveButton.Text = "Save";
				this.SaveButton.UseVisualStyleBackColor = true;
				this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
				// 
				// UnsubscribeButton
				// 
				this.UnsubscribeButton.Enabled = false;
				this.UnsubscribeButton.Location = new System.Drawing.Point(490, 151);
				this.UnsubscribeButton.Name = "UnsubscribeButton";
				this.UnsubscribeButton.Size = new System.Drawing.Size(120, 40);
				this.UnsubscribeButton.TabIndex = 8;
				this.UnsubscribeButton.Text = "Unsubscribe";
				this.UnsubscribeButton.UseVisualStyleBackColor = true;
				this.UnsubscribeButton.Click += new System.EventHandler(this.UnsubscribeButton_Click);
				// 
				// EditCategoryForm
				// 
				this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
				this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
				this.ClientSize = new System.Drawing.Size(622, 204);
				this.Controls.Add(this.UnsubscribeButton);
				this.Controls.Add(this.SaveButton);
				this.Controls.Add(this.DescriptionLabel);
				this.Controls.Add(this.DescriptionTextbox);
				this.Controls.Add(this.NameLabel);
				this.Controls.Add(this.NameTextbox);
				this.Controls.Add(this.LoadAndSubscribeButton);
				this.Controls.Add(this.CategoryIdLabel);
				this.Controls.Add(this.CategoryIdTextbox);
				this.Name = "EditCategoryForm";
				this.Text = "Edit Category";
				this.ResumeLayout(false);
				this.PerformLayout();

			}

			#endregion

			private System.Windows.Forms.TextBox CategoryIdTextbox;
			private System.Windows.Forms.Label CategoryIdLabel;
			private System.Windows.Forms.Button LoadAndSubscribeButton;
			private System.Windows.Forms.Label NameLabel;
			private System.Windows.Forms.TextBox NameTextbox;
			private System.Windows.Forms.Label DescriptionLabel;
			private System.Windows.Forms.TextBox DescriptionTextbox;
			private System.Windows.Forms.Button SaveButton;
			private System.Windows.Forms.Button UnsubscribeButton;
		}
	}
}