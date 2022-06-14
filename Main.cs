using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.Diagnostics;
using RainbowMage.OverlayPlugin;

namespace ComboSkills {
	public class Main : UserControl, IActPluginV1 {
		private Cactbot.CactbotEventSource CactbotEventSrc;
		private InputHook InputHook;

		private string[] Classes = { "ACN", "ARC", "AST", "BLM", "BLU", "BRD", "DNC", "DRG", "DRK", "GLD", "GNB", "LNC", "MCH", "MNK", "MRD", "NIN", "PLD", "PUG", "RDM", "ROG", "RPR", "SAM", "SCH", "SGE", "SMN", "THM", "WAR", "WHM", "BTN", "FSH", "MIN", "ALC", "ARM", "BSM", "CRP", "CUL", "GSM", "LTW", "WVR" };

		#region Form Controls

		private IContainer components = null;
		private Label PluginStatus;

		private TabControl TabControl;
		private TabPage TabPageSkills;
		private TabPage TabPageCombos;
		private TabPage TabPageOther;

		private Label SkillNameLbl;
		private TextBox SkillNameText;
		private Label SkillAltNameLbl;
		private TextBox SkillAltNameText;
		private Label SkillKeyLbl;
		private TextBox SkillKeyText;
		private Label SkillModLbl;
		private CheckBox SkillCtrlModChk;
		private CheckBox SkillShiftModChk;
		private CheckBox SkillAltModChk;
		private Button SkillAddUpdateBtn;
		private Button SkillRemoveBtn;

		private ListView SkillsListView;
		private ColumnHeader SkillsListColName;
		private ColumnHeader SkillsListColAltName;
		private ColumnHeader SkillsListColKey;
		private ColumnHeader SkillsListColCtrlMod;
		private ColumnHeader SkillsListColShiftMod;
		private ColumnHeader SkillsListColAltMod;

		private Label ComboClassLbl;
		private ComboBox ComboClassCmb;
		private Label ComboExpirationLbl;
		private TextBox ComboExpirationText;
		private Label ComboSkillLbl;
		private ComboBox ComboSkillCmb;
		private Button ComboAddRemoveSkillBtn;
		private Label ComboAppendingLbl;
		private TextBox ComboAppendingText;
		private Button ComboAddBtn;
		private Button ComboRemoveBtn;

		private ListView CombosListView;
		private ColumnHeader CombosListColClass;
		private ColumnHeader CombosListColExpiration;
		private ColumnHeader CombosListColSkills;

		private Label PlayerNameLbl;
		private TextBox PlayerNameText;

		#endregion

		public Main() {
			InitializeComponent();
		}

		private void OFormActMain_BeforeCombatAction(bool isImport, CombatActionEventArgs actionInfo) {
			if(actionInfo.attacker.ToLower() == Config.Localization.PlayerName.ToLower()) {
				Skill skill = Config.SkillCombo.Skills.Find(x => x.Name == actionInfo.theAttackType || (!string.IsNullOrEmpty(x.AltName) && x.AltName == actionInfo.theAttackType));

				if(skill != null) {
					Combo combo = Config.SkillCombo.Combos.Find(x => x.Triggered && x.Skills.Contains(skill.Name));

					if(combo != null) {
						if(combo.Skills.IndexOf(skill.Name) + 1 > combo.Skills.Count - 1) {
							combo.Index = 0;
							combo.Triggered = false;
							combo.ExpirationTimestamp = 0;
						} else {
							combo.Index = combo.Skills.IndexOf(skill.Name) + 1;
							combo.ExpirationTimestamp = DateTime.Now.Ticks;
						}
					}
				}
			}
		}

		private void CactbotEventSrc_OnPlayerDied(Cactbot.JSEvents.PlayerDiedEvent e) {
			foreach(Combo combo in Config.SkillCombo.Combos) {
				combo.Index = 0;
				combo.Triggered = false;
				combo.ExpirationTimestamp = 0;
			}
		}

		private void CactbotEventSrc_OnPlayerChanged(Cactbot.JSEvents.PlayerChangedEvent e) {
			if(e.job != Config.Localization.PlayerClass) {
				foreach(Combo combo in Config.SkillCombo.Combos) {
					combo.Index = 0;
					combo.Triggered = false;
					combo.ExpirationTimestamp = 0;
				}

				ActGlobals.oFormActMain.UI(() => {
					if(ComboClassCmb.Items.Contains(e.job)) {
						ComboClassCmb.SelectedItem = e.job;
					}
				});
			}
			Config.Localization.PlayerClass = e.job;
		}

		public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText) {
			PluginStatus = pluginStatusText;
			pluginScreenSpace.Controls.Add(this);
			Dock = DockStyle.Fill;

			Config.Load();
			InputHook = new InputHook();

			Config.Localization.PlayerClass = "";
			foreach(Combo combo in Config.SkillCombo.Combos) {
				combo.Index = 0;
				combo.Triggered = false;
				combo.ExpirationTimestamp = 0;
			}

			TinyIoCContainer container = Registry.GetContainer();
			Registry registry = container.Resolve<Registry>();
			IEventSource eventSrc = registry.EventSources.First(x => x.Name == "Cactbot Config");
			if(eventSrc != null) {
				CactbotEventSrc = (Cactbot.CactbotEventSource)eventSrc;
				CactbotEventSrc.OnPlayerChanged += CactbotEventSrc_OnPlayerChanged;
				CactbotEventSrc.OnPlayerDied += CactbotEventSrc_OnPlayerDied;
            }

			ActGlobals.oFormActMain.BeforeCombatAction += OFormActMain_BeforeCombatAction;
			PluginStatus.Text = "Initialised";

			PopulateSkillsTable();
			PopulateCombosTable();

			PlayerNameText.Text = Config.Localization.PlayerName;

			SkillNameText.BringToFront();
			SkillAltNameText.BringToFront();
			SkillKeyText.BringToFront();
			ComboClassCmb.BringToFront();
			ComboExpirationText.BringToFront();
			ComboSkillCmb.BringToFront();
			ComboAppendingText.BringToFront();
			PlayerNameText.BringToFront();

			SkillsListView.Size = new Size(TabPageSkills.Size.Width - (SkillsListView.Location.X * 2), TabPageSkills.Size.Height - SkillsListView.Location.Y);
			CombosListView.Size = new Size(TabPageCombos.Size.Width - (CombosListView.Location.X * 2), TabPageCombos.Size.Height - CombosListView.Location.Y);
		}

		private void PopulateSkillsTable() {
			Config.SkillCombo.Skills = Config.SkillCombo.Skills.OrderBy(x => x.Name).ToList();

			SkillsListView.Items.Clear();
			foreach(Skill skill in Config.SkillCombo.Skills) {
				SkillsListView.Items.Add(new ListViewItem(new string[] {
					skill.Name,
					skill.AltName,
					skill.Key.ToString(),
					skill.CtrlMod ? "✓" : "",
					skill.ShiftMod ? "✓" : "",
					skill.AltMod ? "✓" : ""
				}));
			}
		}
		private void ClearSkillsFields() {
			SkillNameText.Text = "";
			SkillAltNameText.Text = "";
			SkillKeyText.Text = "";
			SkillCtrlModChk.Checked = false;
			SkillShiftModChk.Checked = false;
			SkillAltModChk.Checked = false;
		}
		private void PopulateCombosTable() {
			Config.SkillCombo.Combos = Config.SkillCombo.Combos.OrderBy(x => x.Class).ToList();

			CombosListView.Items.Clear();
			foreach(Combo combo in Config.SkillCombo.Combos) {
				CombosListView.Items.Add(new ListViewItem(new string[] {
					combo.Class,
					combo.Expiration.ToString(),
					string.Join(" > ", combo.Skills.ToArray())
				}));
			}
		}

		public void DeInitPlugin() {
			if(CactbotEventSrc != null) {
				CactbotEventSrc.OnPlayerChanged -= CactbotEventSrc_OnPlayerChanged;
				CactbotEventSrc.OnPlayerDied -= CactbotEventSrc_OnPlayerDied;
			}
			ActGlobals.oFormActMain.BeforeCombatAction -= OFormActMain_BeforeCombatAction;
			InputHook.Unhook();
			InputHook = null;

			foreach(Combo combo in Config.SkillCombo.Combos) {
				combo.Index = 0;
				combo.Triggered = false;
				combo.ExpirationTimestamp = 0;
			}
			Config.Save(false);

			PluginStatus.Text = "Unloaded";
		}

		private void InitializeComponent() {
			TabControl = new TabControl();
			TabPageSkills = new TabPage();
			TabPageCombos = new TabPage();
			TabPageOther = new TabPage();

			SkillNameLbl = new Label();
			SkillNameText = new TextBox();
			SkillAltNameLbl = new Label();
			SkillAltNameText = new TextBox();
			SkillKeyLbl = new Label();
			SkillKeyText = new TextBox();
			SkillModLbl = new Label();
			SkillCtrlModChk = new CheckBox();
			SkillShiftModChk = new CheckBox();
			SkillAltModChk = new CheckBox();
			SkillAddUpdateBtn = new Button();
			SkillRemoveBtn = new Button();

			SkillsListView = new ListView();
			SkillsListColName = new ColumnHeader();
			SkillsListColAltName = new ColumnHeader();
			SkillsListColKey = new ColumnHeader();
			SkillsListColCtrlMod = new ColumnHeader();
			SkillsListColShiftMod = new ColumnHeader();
			SkillsListColAltMod = new ColumnHeader();

			ComboClassLbl = new Label();
			ComboClassCmb = new ComboBox();
			ComboExpirationLbl = new Label();
			ComboExpirationText = new TextBox();
			ComboSkillLbl = new Label();
			ComboSkillCmb = new ComboBox();
			ComboAddRemoveSkillBtn = new Button();
			ComboAppendingLbl = new Label();
			ComboAppendingText = new TextBox();
			ComboAddBtn = new Button();
			ComboRemoveBtn = new Button();

			CombosListView = new ListView();
			CombosListColClass = new ColumnHeader();
			CombosListColExpiration = new ColumnHeader();
			CombosListColSkills = new ColumnHeader();

			PlayerNameLbl = new Label();
			PlayerNameText = new TextBox();

			TabControl.SuspendLayout();
			TabPageSkills.SuspendLayout();
			TabPageCombos.SuspendLayout();
			TabPageOther.SuspendLayout();
			SuspendLayout();

			#region Tabs

			TabControl.Controls.Add(TabPageSkills);
			TabControl.Controls.Add(TabPageCombos);
			TabControl.Controls.Add(TabPageOther);
			TabControl.Dock = DockStyle.Fill;
			TabControl.Location = new Point(0, 0);
			TabControl.Name = "TabControl";
			TabControl.SelectedIndex = 0;

			TabPageSkills.Controls.Add(SkillNameLbl);
			TabPageSkills.Controls.Add(SkillNameText);
			TabPageSkills.Controls.Add(SkillAltNameLbl);
			TabPageSkills.Controls.Add(SkillAltNameText);
			TabPageSkills.Controls.Add(SkillKeyLbl);
			TabPageSkills.Controls.Add(SkillKeyText);
			TabPageSkills.Controls.Add(SkillModLbl);
			TabPageSkills.Controls.Add(SkillCtrlModChk);
			TabPageSkills.Controls.Add(SkillShiftModChk);
			TabPageSkills.Controls.Add(SkillAltModChk);
			TabPageSkills.Controls.Add(SkillAddUpdateBtn);
			TabPageSkills.Controls.Add(SkillRemoveBtn);
			TabPageSkills.Controls.Add(SkillsListView);
			TabPageSkills.Name = "TabPageSkills";
			TabPageSkills.Padding = new Padding(3);
			TabPageSkills.Text = "Skills";
			TabPageSkills.UseVisualStyleBackColor = true;

			TabPageCombos.Controls.Add(ComboClassLbl);
			TabPageCombos.Controls.Add(ComboClassCmb);
			TabPageCombos.Controls.Add(ComboExpirationLbl);
			TabPageCombos.Controls.Add(ComboExpirationText);
			TabPageCombos.Controls.Add(ComboSkillLbl);
			TabPageCombos.Controls.Add(ComboSkillCmb);
			TabPageCombos.Controls.Add(ComboAddRemoveSkillBtn);
			TabPageCombos.Controls.Add(ComboAppendingLbl);
			TabPageCombos.Controls.Add(ComboAppendingText);
			TabPageCombos.Controls.Add(ComboAddBtn);
			TabPageCombos.Controls.Add(ComboRemoveBtn);
			TabPageCombos.Controls.Add(CombosListView);
			TabPageCombos.Name = "TabPageCombos";
			TabPageCombos.Padding = new Padding(3);
			TabPageCombos.Text = "Combos";
			TabPageCombos.UseVisualStyleBackColor = true;

			TabPageOther.Controls.Add(PlayerNameLbl);
			TabPageOther.Controls.Add(PlayerNameText);
			TabPageOther.Name = "TabPageOther";
			TabPageOther.Text = "Other";
			TabPageOther.UseVisualStyleBackColor = true;

			#endregion

			#region Skills

			SkillNameLbl.AutoSize = true;
			SkillNameLbl.Location = new Point(8, 3);
			SkillNameLbl.Name = "SkillNameLbl";
			SkillNameLbl.Size = new Size(57, 13);
			SkillNameLbl.Text = "Skill Name";

			SkillNameText.Location = new Point(14, 19);
			SkillNameText.Name = "SkillNameText";
			SkillNameText.Size = new Size(138, 20);
			SkillNameText.TextAlign = HorizontalAlignment.Center;

			SkillAltNameLbl.AutoSize = true;
			SkillAltNameLbl.Location = new Point(8, 44);
			SkillAltNameLbl.Name = "SkillAltNameLbl";
			SkillAltNameLbl.Size = new Size(88, 13);
			SkillAltNameLbl.Text = "Alternative Name";

			SkillAltNameText.Location = new Point(14, 60);
			SkillAltNameText.Name = "SkillAltNameText";
			SkillAltNameText.Size = new Size(138, 20);
			SkillAltNameText.TextAlign = HorizontalAlignment.Center;

			SkillKeyLbl.AutoSize = true;
			SkillKeyLbl.Location = new Point(152, 3);
			SkillKeyLbl.Name = "SkillKeyLbl";
			SkillKeyLbl.Size = new Size(63, 13);
			SkillKeyLbl.Text = "Key Binding";

			SkillKeyText.Location = new Point(158, 19);
			SkillKeyText.Name = "SkillKeyText";
			SkillKeyText.Size = new Size(138, 20);
			SkillKeyText.TextAlign = HorizontalAlignment.Center;

			SkillModLbl.AutoSize = true;
			SkillModLbl.Location = new Point(152, 44);
			SkillModLbl.Name = "SkillModLbl";
			SkillModLbl.Size = new Size(70, 13);
			SkillModLbl.Text = "Modifier Keys";

			SkillCtrlModChk.AutoSize = true;
			SkillCtrlModChk.Location = new Point(158, 62);
			SkillCtrlModChk.Name = "SkillCtrlModChk";
			SkillCtrlModChk.Size = new Size(41, 17);
			SkillCtrlModChk.Text = "Ctrl";
			SkillCtrlModChk.UseVisualStyleBackColor = true;

			SkillShiftModChk.AutoSize = true;
			SkillShiftModChk.Location = new Point(205, 62);
			SkillShiftModChk.Name = "SkillShiftModChk";
			SkillShiftModChk.Size = new Size(47, 17);
			SkillShiftModChk.Text = "Shift";
			SkillShiftModChk.UseVisualStyleBackColor = true;

			SkillAltModChk.AutoSize = true;
			SkillAltModChk.Location = new Point(258, 62);
			SkillAltModChk.Name = "SkillAltModChk";
			SkillAltModChk.Size = new Size(38, 17);
			SkillAltModChk.Text = "Alt";
			SkillAltModChk.UseVisualStyleBackColor = true;

			SkillAddUpdateBtn.Location = new Point(302, 19);
			SkillAddUpdateBtn.Name = "SkillAddUpdateBtn";
			SkillAddUpdateBtn.Size = new Size(138, 23);
			SkillAddUpdateBtn.Text = "Add/Update Skill";
			SkillAddUpdateBtn.UseVisualStyleBackColor = true;
			SkillAddUpdateBtn.Click += (s, e) => {
				if(SkillNameText.Text != "" && SkillKeyText.Text != "") {
					string skillName = SkillNameText.Text;
					string altName = SkillAltNameText.Text;
					Keys key = Keys.None;
					Enum.TryParse(SkillKeyText.Text, out key);
					if(key == Keys.None) { return; }
					bool modCtrl = SkillCtrlModChk.Checked;
					bool modShift = SkillShiftModChk.Checked;
					bool modAlt = SkillAltModChk.Checked;

					if(Config.SkillCombo.Skills.Find(x => x.Name == skillName && (string.IsNullOrEmpty(x.AltName) || x.AltName == altName) && x.Key == key && x.CtrlMod == modCtrl && x.ShiftMod == modShift && x.AltMod == modAlt) == null) {
						if(SkillsListView.SelectedItems.Count > 0) {
							string prevName = SkillsListView.SelectedItems[0].SubItems[0].Text;
							string prevAltName = SkillsListView.SelectedItems[0].SubItems[1].Text;
							Keys prevKey = Keys.None;
							Enum.TryParse(SkillsListView.SelectedItems[0].SubItems[2].Text, out prevKey);
							bool prevCtrl = SkillsListView.SelectedItems[0].SubItems[3].Text == "✓";
							bool prevShift = SkillsListView.SelectedItems[0].SubItems[4].Text == "✓";
							bool prevAlt = SkillsListView.SelectedItems[0].SubItems[5].Text == "✓";

							Skill skill = Config.SkillCombo.Skills.Find(x => x.Name == prevName && (string.IsNullOrEmpty(x.AltName) || x.AltName == prevAltName) && x.Key == prevKey && x.CtrlMod == prevCtrl && x.ShiftMod == prevShift && x.AltMod == prevAlt);
							if(skill != null) {
								skill.Name = skillName;
								skill.AltName = altName;
								skill.Key = key;
								skill.CtrlMod = modCtrl;
								skill.ShiftMod = modShift;
								skill.AltMod = modAlt;
								Config.Save(true);
								ClearSkillsFields();
								PopulateSkillsTable();
							}
						} else {
							Debug.WriteLine("NOT SELECTED");
							Config.SkillCombo.Skills.Add(new Skill { Name = skillName, AltName = altName, Key = key, CtrlMod = modCtrl, ShiftMod = modShift, AltMod = modAlt });
							Config.Save(true);
							ClearSkillsFields();
							PopulateSkillsTable();
						}
					}
				}
			};

			SkillRemoveBtn.Location = new Point(302, 56);
			SkillRemoveBtn.Name = "SkillRemoveBtn";
			SkillRemoveBtn.Size = new Size(138, 23);
			SkillRemoveBtn.Text = "Remove Skill";
			SkillRemoveBtn.UseVisualStyleBackColor = true;
			SkillRemoveBtn.Click += (s, e) => {
				if(SkillsListView.SelectedItems.Count > 0) {
					Skill skill = Config.SkillCombo.Skills.Find(x => x.Name == SkillsListView.SelectedItems[0].SubItems[0].Text);
					if(skill != null) {
						Config.SkillCombo.Skills.Remove(skill);
						Config.Save(true);
						ClearSkillsFields();
						PopulateSkillsTable();
					}
				}
			};

			SkillsListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			SkillsListView.Columns.AddRange(new ColumnHeader[] { SkillsListColName, SkillsListColAltName, SkillsListColKey, SkillsListColCtrlMod, SkillsListColShiftMod, SkillsListColAltMod });
			SkillsListView.FullRowSelect = true;
			SkillsListView.GridLines = true;
			SkillsListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			SkillsListView.HideSelection = false;
			SkillsListView.LabelWrap = false;
			SkillsListView.Location = new Point(8, 86);
			SkillsListView.MultiSelect = false;
			SkillsListView.Name = "SkillsListView";
			SkillsListView.UseCompatibleStateImageBehavior = false;
			SkillsListView.View = View.Details;
			SkillsListView.Scrollable = true;
			SkillsListView.SelectedIndexChanged += (s, e) => {
				if(SkillsListView.SelectedItems.Count > 0) {
					SkillNameText.Text = SkillsListView.SelectedItems[0].SubItems[0].Text;
					SkillAltNameText.Text = SkillsListView.SelectedItems[0].SubItems[1].Text;
					SkillKeyText.Text = SkillsListView.SelectedItems[0].SubItems[2].Text;
					SkillCtrlModChk.Checked = SkillsListView.SelectedItems[0].SubItems[3].Text == "✓";
					SkillShiftModChk.Checked = SkillsListView.SelectedItems[0].SubItems[4].Text == "✓";
					SkillAltModChk.Checked = SkillsListView.SelectedItems[0].SubItems[5].Text == "✓";
				} else {
					ClearSkillsFields();
				}
			};

			SkillsListColName.Text = "Name";
			SkillsListColName.Width = 121;
			SkillsListColAltName.Text = "AltName";
			SkillsListColAltName.Width = 101;
			SkillsListColKey.Text = "Key";
			SkillsListColKey.TextAlign = HorizontalAlignment.Center;
			SkillsListColCtrlMod.Text = "Ctrl";
			SkillsListColCtrlMod.TextAlign = HorizontalAlignment.Center;
			SkillsListColCtrlMod.Width = 35;
			SkillsListColShiftMod.Text = "Shift";
			SkillsListColShiftMod.TextAlign = HorizontalAlignment.Center;
			SkillsListColShiftMod.Width = 35;
			SkillsListColAltMod.Text = "Alt";
			SkillsListColAltMod.TextAlign = HorizontalAlignment.Center;
			SkillsListColAltMod.Width = 31;

			#endregion

			#region Combos

			ComboClassLbl.AutoSize = true;
			ComboClassLbl.Location = new Point(8, 3);
			ComboClassLbl.Name = "ComboClassLbl";
			ComboClassLbl.Size = new Size(32, 13);
			ComboClassLbl.Text = "Class";

			ComboClassCmb.FormattingEnabled = true;
			ComboClassCmb.Location = new Point(14, 19);
			ComboClassCmb.Name = "ComboClassCmb";
			ComboClassCmb.Size = new Size(69, 21);
			ComboClassCmb.DropDownStyle = ComboBoxStyle.DropDownList;
			ComboClassCmb.Items.AddRange(Classes);

			ComboExpirationLbl.AutoSize = true;
			ComboExpirationLbl.Location = new Point(83, 3);
			ComboExpirationLbl.Name = "ComboExpirationLbl";
			ComboExpirationLbl.Size = new Size(53, 13);
			ComboExpirationLbl.Text = "Expiration";

			ComboExpirationText.Location = new Point(89, 19);
			ComboExpirationText.Name = "ComboExpirationText";
			ComboExpirationText.Size = new Size(69, 20);
			ComboExpirationText.TextAlign = HorizontalAlignment.Center;
			ComboExpirationText.Text = "0";

			ComboSkillLbl.AutoSize = true;
			ComboSkillLbl.Location = new Point(158, 3);
			ComboSkillLbl.Name = "ComboSkillLbl";
			ComboSkillLbl.Size = new Size(59, 13);
			ComboSkillLbl.Text = "Select Skill";

			ComboSkillCmb.FormattingEnabled = true;
			ComboSkillCmb.Location = new Point(164, 19);
			ComboSkillCmb.Name = "ComboSkillCmb";
			ComboSkillCmb.Size = new Size(138, 21);
			ComboSkillCmb.DropDownStyle = ComboBoxStyle.DropDownList;
			ComboSkillCmb.Click += (s, e) => {
				ComboSkillCmb.Items.Clear();
				foreach(Skill skill in Config.SkillCombo.Skills) {
					ComboSkillCmb.Items.Add(skill.Name);
				}
			};

			ComboAddRemoveSkillBtn.Location = new Point(308, 19);
			ComboAddRemoveSkillBtn.Name = "ComboAddRemoveSkillBtn";
			ComboAddRemoveSkillBtn.Size = new Size(138, 22);
			ComboAddRemoveSkillBtn.Text = "Add/Remove Skill";
			ComboAddRemoveSkillBtn.UseVisualStyleBackColor = true;
			ComboAddRemoveSkillBtn.Click += (s, e) => {
				if(ComboSkillCmb.SelectedItem != null) {
					List<string> skills = ComboAppendingText.Text.Split(new string[] { " > " }, StringSplitOptions.RemoveEmptyEntries).ToList();

					if(ComboAppendingText.Text.Contains(ComboSkillCmb.SelectedItem.ToString())) {
						skills.Remove(ComboSkillCmb.SelectedItem.ToString());
					} else {
						skills.Add(ComboSkillCmb.SelectedItem.ToString());
					}

					ComboAppendingText.Text = string.Join(" > ", skills.ToArray());
				}
			};

			ComboAppendingLbl.AutoSize = true;
			ComboAppendingLbl.Location = new Point(8, 44);
			ComboAppendingLbl.Name = "ComboAppendingLbl";
			ComboAppendingLbl.Size = new Size(74, 13);
			ComboAppendingLbl.Text = "Combo Sequence";

			ComboAppendingText.Location = new Point(14, 60);
			ComboAppendingText.Name = "ComboAppendingText";
			ComboAppendingText.ReadOnly = true;
			ComboAppendingText.Size = new Size(432, 20);
			ComboAppendingText.TextAlign = HorizontalAlignment.Center;

			ComboAddBtn.Location = new Point(452, 19);
			ComboAddBtn.Name = "ComboAddBtn";
			ComboAddBtn.Size = new Size(138, 23);
			ComboAddBtn.Text = "Add/Update Combo";
			ComboAddBtn.UseVisualStyleBackColor = true;
			ComboAddBtn.Click += (s, e) => {
				if(ComboAppendingText.Text != "") {
					string job = ComboClassCmb.SelectedItem.ToString();
					int expiration = 0;
					int.TryParse(ComboExpirationText.Text, out expiration);
					List<string> skills = ComboAppendingText.Text.Split(new string[] { " > " }, StringSplitOptions.RemoveEmptyEntries).ToList();

					if(Config.SkillCombo.Combos.Find(x => x.Class == job && x.Expiration == expiration && x.Skills.SequenceEqual(skills)) == null) {
						if(CombosListView.SelectedItems.Count > 0) {
							string prevJob = CombosListView.SelectedItems[0].SubItems[0].Text;
							string prevExpiration = CombosListView.SelectedItems[0].SubItems[1].Text;
							List<string> prevSkills = CombosListView.SelectedItems[0].SubItems[2].Text.Split(new string[] { " > " }, StringSplitOptions.RemoveEmptyEntries).ToList();

							Combo combo = Config.SkillCombo.Combos.Find(x => x.Class == prevJob && x.Expiration == int.Parse(prevExpiration) && x.Skills.SequenceEqual(prevSkills));
							if(combo != null) {
								combo.Class = job;
								combo.Expiration = expiration;
								combo.Skills = skills;
								Config.Save(true);
								ComboAppendingText.Text = "";
								PopulateCombosTable();
							}
						} else {
							Config.SkillCombo.Combos.Add(new Combo { Class = job, Expiration = expiration, Skills = skills });
							Config.Save(true);
							ComboAppendingText.Text = "";
							PopulateCombosTable();
						}
					}
				}
			};

			ComboRemoveBtn.Location = new Point(452, 58);
			ComboRemoveBtn.Name = "ComboRemoveBtn";
			ComboRemoveBtn.Size = new Size(138, 23);
			ComboRemoveBtn.Text = "Remove Combo";
			ComboRemoveBtn.UseVisualStyleBackColor = true;
			ComboRemoveBtn.Click += (s, e) => {
				if(CombosListView.SelectedItems.Count > 0) {
					string job = CombosListView.SelectedItems[0].SubItems[0].Text;
					string expiration = CombosListView.SelectedItems[0].SubItems[1].Text;
					List<string> skills = CombosListView.SelectedItems[0].SubItems[2].Text.Split(new string[] { " > " }, StringSplitOptions.RemoveEmptyEntries).ToList();

					Combo combo = Config.SkillCombo.Combos.Find(x => x.Class == job && x.Expiration == int.Parse(expiration) && x.Skills.SequenceEqual(skills));
					if(combo != null) {
						Config.SkillCombo.Combos.Remove(combo);
						Config.Save(true);
						ComboAppendingText.Text = "";
						PopulateCombosTable();
					}
				}
			};

			CombosListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			CombosListView.Columns.AddRange(new ColumnHeader[] { CombosListColClass, CombosListColExpiration, CombosListColSkills });
			CombosListView.FullRowSelect = true;
			CombosListView.GridLines = true;
			CombosListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			CombosListView.HideSelection = false;
			CombosListView.LabelWrap = false;
			CombosListView.Location = new Point(8, 86);
			CombosListView.MultiSelect = false;
			CombosListView.Name = "CombosListView";
			CombosListView.UseCompatibleStateImageBehavior = false;
			CombosListView.View = View.Details;
			CombosListView.Scrollable = true;
			CombosListView.SelectedIndexChanged += (s, e) => {
				if(CombosListView.SelectedItems.Count > 0) {
					ComboClassCmb.SelectedItem = CombosListView.SelectedItems[0].SubItems[0].Text;
					ComboExpirationText.Text = CombosListView.SelectedItems[0].SubItems[1].Text;
					ComboAppendingText.Text = CombosListView.SelectedItems[0].SubItems[2].Text;
				} else {
					ComboAppendingText.Text = "";
				}
			};

			CombosListColClass.Text = "Class";
			CombosListColClass.Width = 40;
			CombosListColExpiration.Text = "Expiration";
			CombosListColExpiration.TextAlign = HorizontalAlignment.Center;
			CombosListColExpiration.Width = 58;
			CombosListColSkills.Text = "Skills";
			CombosListColSkills.TextAlign = HorizontalAlignment.Center;
			CombosListColSkills.Width = 550;

			#endregion

			#region Other

			PlayerNameLbl.AutoSize = true;
			PlayerNameLbl.Location = new Point(8, 3);
			PlayerNameLbl.Name = "PlayerNameLbl";
			PlayerNameLbl.Size = new Size(138, 13);
			PlayerNameLbl.Text = "Player Name in Combat Log";

			PlayerNameText.Location = new Point(14, 19);
			PlayerNameText.Name = "PlayerNameText";
			PlayerNameText.Size = new Size(138, 20);
			PlayerNameText.Text = "YOU";
			PlayerNameText.TextAlign = HorizontalAlignment.Center;
			PlayerNameText.TextChanged += (s, e) => {
				Config.Localization.PlayerName = PlayerNameText.Text;
				Config.Save(true);
			};

			#endregion

			AutoScaleDimensions = new SizeF(6F, 13F);
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(TabControl);
			Name = "ComboSkillsPlugin";
			TabControl.ResumeLayout(false);
			TabPageSkills.ResumeLayout(false);
			TabPageSkills.PerformLayout();
			TabPageCombos.ResumeLayout(false);
			TabPageCombos.PerformLayout();
			TabPageOther.ResumeLayout(false);
			TabPageOther.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
