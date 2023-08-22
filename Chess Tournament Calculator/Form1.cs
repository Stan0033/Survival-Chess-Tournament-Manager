using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
namespace Chess_Tournament_Calculator
{
    public partial class Form1 : Form
    {
        List<Player>? Players;
        int OddPlayers;
        int Current_Round;
        int count_games_this_round;
        int RoundGamesPlayed = 0;
        List<string> schedule = new();
        bool shuffleColor = true;
        bool Flip;
        string test;
        public Form1()
        {
            InitializeComponent();
            Players = new List<Player>();
            Current_Round = 0;
            //schedule = new List<string>();
            count_games_this_round = 0;
            OddPlayers = 0;
            Flip = true;
            test = string.Empty;
        }
        private void Add_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            bool ValidName = ValidateInput(name);
            if (!ValidName) { return; }
            bool met = false;
            foreach (string field in listBox_players.Items)
            {
                if (field == name) { met = true; }
            }
            if (met) { MessageBox.Show("already entered"); }
            else
            {
                listBox_players.Items.Add(name);
            }
            textBox1.Text = string.Empty;
            if (listBox_players.Items.Count >= 4)
            {
                button_start_tournament.Enabled = true;
            }
            else
            {
                button_start_tournament.Enabled = false;
            }
        }
        private bool ValidateInput(string input)
        {
            // Check for empty input
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Input cannot be empty.");
                return false;
            }
            // Check for numbers or symbols
            if (input.Any(c => !char.IsLetter(c) && !char.IsWhiteSpace(c)))
            {
                MessageBox.Show("Input cannot contain numbers or symbols.");
                return false;
            }
            // Check length
            if (input.Length <= 6)
            {
                MessageBox.Show("Input must be longer than 6 characters.");
                return false;
            }
            // Check word count
            int wordCount = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount < 2 || wordCount > 3)
            {
                MessageBox.Show("Input must have 2 or 3 words.");
                return false;
            }
            return true;
        }
        private void Edit_Click(object sender, EventArgs e)
        {
            if (listBox_players.SelectedItems.Count == 1)
            {
                string name = textBox1.Text;
                bool ValidName = ValidateInput(name);
                if (ValidName)
                {
                    int pos = listBox_players.SelectedIndex;
                    listBox_players.Items[pos] = textBox1.Text;
                    textBox1.Text = string.Empty;
                }
            }
        }
        private void Delete_Click(object sender, EventArgs e)
        {
            if (listBox_players.SelectedItems.Count == 1)
            {
                int pos = listBox_players.SelectedIndex;
                listBox_players.Items.RemoveAt(pos);
            }
            if (listBox_players.Items.Count >= 4)
            {
                button_start_tournament.Enabled = true;
            }
            else
            {
                button_start_tournament.Enabled = false;
            }
        }
        private void Form_Load(object sender, EventArgs e)
        {
            tabControl1.TabPages[1].Enabled = false;
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox_players.SelectedIndex;
            if (index >= 0)
            {
                textBox1.Text = listBox_players.Items[index].ToString();
            }
        }
        private void EnterName_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Enter))
            {
                string name = textBox1.Text;
                bool ValidName = ValidateInput(name);
                if (!ValidName) { return; }
                bool met = false;
                foreach (string field in listBox_players.Items)
                {
                    if (field == name) { met = true; }
                }
                if (met) { MessageBox.Show("already entered"); }
                else
                {
                    listBox_players.Items.Add(name);
                }
                textBox1.Text = string.Empty;
                if (listBox_players.Items.Count >= 4)
                {
                    button_start_tournament.Enabled = true;
                }
                else
                {
                    button_start_tournament.Enabled = false;
                }
            }
        }
        private void Start_Tournament_click(object sender, EventArgs e)
        {
            tabControl1.TabPages[0].Enabled = false;
            tabControl1.TabPages[1].Enabled = true;
            tabControl1.SelectedTab = tabControl1.TabPages[1];
            //-------------------------------------------------------
            Players.Clear();
            // fill the players from the listbox to the players list
            
            foreach (string name in listBox_players.Items) { Players.Add(new Player(name, shuffleColor)); shuffleColor = !shuffleColor; }
            // Handle odd player count
            OddPlayers = Players.Count % 2 == 0 ? 0 : 1;
            schedule = GenerateSurvivalChessSchedule(Players);
            Refresh_Standings(); // if tehre is an odd player, he will appear as having a win
            RoundGamesPlayed = 1;
            tabControl1.TabPages[1].Text = $"Round {Current_Round + 1}"; // Update the tab title
            GenerateButtonsForPlayers(Current_Round);
            if (Players.Count == 1)
            {
                tabControl1.TabPages[1].Text = "Tournament is over";
                return;
            }
        }
        public void GenerateButtonsForPlayers(int whichRound)
        {
            int forname = 0;
            int currentPosition = 10;
            foreach (string pair in schedule)
            {
                if (pair.Contains("BYE")) // if a player has  bye he gets a win
                {
                    continue;
                }
                ButtonForPair button = new();
                //group_CurrentPairs.Controls.Add(button);
                button.Height = 50;
                button.Width = 500;
                button.Location = new Point(0, currentPosition);
                currentPosition += 50;
                button.Text = pair;
                button.Enabled = true;
                button.Visible = true;
                button.ForeColor = Color.White;
                button.Click += DecideResultOfPair;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
                button.Name = Convert.ToString(forname);
                button.Cursor = Cursors.Hand;
                forname++;
                panel_Pairings.Controls.Add(button);
            }
        }
        public void Refresh_Standings()
        {
            List<string> player_scores = new List<string>();
            // Sort players by whether they are eliminated (eliminated players come last),
            // and then within each group, sort by score
            var sortedPlayers = Players.OrderBy(player => player.Eliminated)
                                      .ThenByDescending(player => CalculatePlayerScore(player.Name, player.Games));
            int position = 1;
            foreach (var player in sortedPlayers)
            {
                double score = CalculatePlayerScore(player.Name, player.Games);
                string eliminatedMarker = player.Eliminated ? " (Eliminated)" : "";
                string formatted = $"{position++}. {player.Name} - {score:f1}{eliminatedMarker}";
                player_scores.Add(formatted);
            }
            listBox_standings.Items.Clear();
            foreach (string standing in player_scores)
            {
                listBox_standings.Items.Add(standing);
            }
        }
        // Calculate player score based on games
        private double CalculatePlayerScore(string playerName, List<Game> games)
        {
            double score = 0;
            foreach (var game in games)
            {
                if (game.Player_White == playerName)
                {
                    if (game.Result == 1) { score += 1; }
                }
                if (game.Player_Black == playerName)
                {
                    if (game.Result == 0) { score += 1; }
                }
                if (game.Result == 0.5) { score += 0.5; }
            }
            return score;
        }
        private void ViewPairingsTXT_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages[1].Text.Contains("over"))
            {
                return;
            }
            string filename = $"rr_schedule_{Current_Round + 1}.txt"; // Incremented the round number
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine($"Pairings for round {Current_Round + 1}:\n"); // Incremented the round number
                foreach (var pair in schedule)
                {
                    writer.WriteLine(pair);
                }
            }
            Process.Start("notepad.exe", filename); // Run the external file
        }
        private void ViewCompletedGamesTXT_Click(object sender, EventArgs e)
        {

            string content = GenerateRoundsAndGamesString(Players);
            string filename = "past_games.txt";
            // Write the content to the file and open it
            File.WriteAllText(filename, content);
            Process.Start("notepad.exe", filename);
        }
        public string GenerateRoundsAndGamesString(List<Player> players)
        {
            StringBuilder sb = new StringBuilder();
            int maxRounds = players.Max(player => player.Games.Count);
            HashSet<Game> displayedGames = new HashSet<Game>();

            for (int i = 0; i < maxRounds; i++)
            {
                sb.AppendLine($"Round {i + 1}:");

                foreach (Player player in players)
                {
                    if (i < player.Games.Count)
                    {
                        Game game = player.Games[i];
                        if (!displayedGames.Contains(game))
                        {
                            sb.AppendLine(game.ToString());
                            displayedGames.Add(game);
                        }
                    }
                }

                displayedGames.Clear(); // Clear the set for the next round
            }

            return sb.ToString();
        }

        private void enterNamesEnMass_Click(object sender, EventArgs e)
        {
            using (var v = new enterlines())
            {
                v.ShowDialog();
                if (v.DialogResult == DialogResult.OK)
                {
                    string lines = v.richTextBox1.Text;
                    ProcessLines(lines);
                    if (listBox_players.Items.Count >= 4)
                    {
                        button_start_tournament.Enabled = true;
                    }
                }
            }
        }
        public void ProcessLines(string lines)
        {
            string[] inputLines = lines.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in inputLines)
            {
                string trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    string[] words = trimmedLine.Split(' ');
                    if (words.Length >= 2 && words.Length <= 3)
                    {
                        bool isValid = true;
                        List<string> errorMessages = new List<string>();
                        foreach (string word in words)
                        {
                            if (!IsCapitalizedWord(word))
                            {
                                isValid = false;
                                errorMessages.Add("Each word must start with a capital letter.");
                            }
                            if (!ContainsOnlyLetters(word))
                            {
                                isValid = false;
                                errorMessages.Add("Each word must contain only letters.");
                            }
                        }
                        if (isValid)
                        {
                            string name = string.Join(" ", words);
                            if (!listBox_players.Items.Contains(name))
                            {
                                listBox_players.Items.Add(name);
                            }
                        }
                        else
                        {
                            string errorMessage = "Invalid line: " + line + Environment.NewLine;
                            errorMessage += string.Join(Environment.NewLine, errorMessages.Distinct());
                            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid line: " + line + Environment.NewLine +
                                        "Line must contain 2 to 3 words separated by spaces.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // Update the start tournament button state based on player count
            if (listBox_players.Items.Count >= 4)
            {
                button_start_tournament.Enabled = true;
            }
            else
            {
                button_start_tournament.Enabled = false;
            }
        }
        private static bool IsCapitalizedWord(string word)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return word.Length > 0 && char.IsUpper(word[0]) && textInfo.ToUpper(word[0]) == word[0];
        }
        private static bool ContainsOnlyLetters(string word)
        {
            return word.All(char.IsLetter);
        }
        private void showGamesOnRankingsListClick(object sender, MouseEventArgs e)
        {
            if (listBox_standings.SelectedItems.Count == 1)
            {
                int index = listBox_standings.SelectedIndex;
                if (index < 0) { return; }
                string name = listBox_standings.Items[index].ToString().Split(" - ")[0].Split(". ")[1].Split('(')[0];
                var dialog = new Games_Display();
                dialog.Text = "Games of " + name;
                foreach (Player player in Players)
                {
                    if (player.Name == name)
                    {
                        dialog.games_of_player = (player.Games);
                        break;
                    }
                }
                dialog.ShowDialog();
            }
        }
        //---------------------------------------------------------------------------
        //---------------------------------------------------------------------------
        public void EliminatePlayerByName(string playerName, bool isDisqualified)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name == playerName)
                {
                    Players[i].Eliminated = true;
                    if (isDisqualified)
                    {
                        Players[i].Games.Clear();
                    }
                    break;
                }

            }
        }
        List<string> GenerateSurvivalChessSchedule(List<Player> players)
        {
            List<string> schedule = new List<string>();
            // Filter out eliminated players
            var activePlayers = players.Where(player => !player.Eliminated).ToList();
            int playerCount = activePlayers.Count;

            // Shuffle the active players to create randomized pairs
            Random random = new Random();
            activePlayers = activePlayers.OrderBy(player => random.Next()).ToList();

            if (playerCount % 2 != 0)
            {
                activePlayers.Add(new Player { Name = "BYE" });
                playerCount++;
            }

            if (playerCount == 2)
            {
                schedule.Add($"{activePlayers[0].Name} - {activePlayers[1].Name}");
            }
            else
            {
                int halfPlayerCount = playerCount / 2;
                for (int i = 0; i < halfPlayerCount; i++)
                {
                    int player1Index = i;
                    int player2Index = (halfPlayerCount + i) % playerCount;
                    string match = string.Empty;

                    string Player1Name = activePlayers[player1Index].Name;
                    string Player2Name = activePlayers[player2Index].Name;

                    // Flipping logic remains unchanged
                    
                    if (Flip)
                    {
                        match = $"{Player2Name} - {Player1Name}";
                    }
                    else
                    {
                        match = $"{Player1Name} - {Player2Name}";
                    }
                    Flip = !Flip;
                    schedule.Add(match);
                }
            }

            // Rest of your code...
            // ...

            return schedule;
        }

        public void DecideResultOfPair(object sender, EventArgs e)
        {
            ButtonForPair clickedButton = (ButtonForPair)sender;
            using (var dg = new enterresult())
            {
                dg.ShowDialog(this);
                if (dg.DialogResult == DialogResult.OK)
                {
                    string whitePlayerName = clickedButton.Text.Split(" - ")[0];
                    string blackPlayerName = clickedButton.Text.Split(" - ")[1];
                    bool isDisqualified = false;
                    
                      
                    Game gameToBeAdded = new()
                    {
                        Player_White = whitePlayerName,
                        Player_Black = blackPlayerName,
                        PGN = dg.PGN
                    };
                    if (dg.button_whitewin.Checked)
                    {
                        gameToBeAdded.Result = 1.0;
                        gameToBeAdded.End_Reason = dg.reason;
                        if (dg.reason == "Disqualification") { isDisqualified = true; }
                            EliminatePlayerByName(blackPlayerName, isDisqualified);
                    }
                    if (dg.button_blackwin.Checked)
                    {
                        gameToBeAdded.Result = 0;
                        gameToBeAdded.End_Reason = dg.reason;
                        if (dg.reason == "Disqualification") { isDisqualified = true; }
                        EliminatePlayerByName(whitePlayerName, isDisqualified);
                    }
                    if (dg.button_draw.Checked)
                    {
                        gameToBeAdded.Result = 0.5;
                        gameToBeAdded.End_Reason = dg.reason;
                    }
                    schedule.RemoveAll(str => str == clickedButton.Text);
                    // add the game to both players lists of games
                    foreach (Player player in Players)
                    {
                        if (player.Name == whitePlayerName || player.Name == blackPlayerName)
                        {
                            player.Games.Add(gameToBeAdded);
                        }

                    }
               
                    //--------------------------------------
                    Refresh_Standings();
                    panel_Pairings.Controls.Remove(clickedButton);

                    // debug.Text = string.Join("\n", Players.Where(player => !player.Eliminated).Select(player => player.Name));
                    //--------------------------------------
                    if (schedule.Count == 1 && schedule[0].Contains("BYE"))
                    {
                        schedule.Clear();
                    }

                    if (schedule.Count == 0)
                    {

                        int playerActive = Players.Count(p => p.Eliminated == false);
                        if (playerActive == 1)
                        {
                            tabControl1.TabPages[1].Text = "Tournament is over";
                        }
                        else
                        {
                            Current_Round++;
                            Players.RemoveAll(player => player.Name == "BYE");//remove bye before ree-evaluating
                            schedule.Clear();
                            schedule = GenerateSurvivalChessSchedule(Players);
                            GenerateButtonsForPlayers(Current_Round + 1);
                            tabControl1.TabPages[1].Text = $"Round {Current_Round + 1}";
                        }

                    }


                }
            }
        }
    }
}
