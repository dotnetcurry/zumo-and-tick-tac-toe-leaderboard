using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DncLeaderboardDemo
{

    public class LeaderBoard
    {
        public int Id { get; set; }

        [DataMember(Name = "gameId")]
        public string GameName { get; set; }

        [DataMember(Name = "gameStatus")]
        public int GameStatus { get; set; }

        [DataMember(Name = "wonBy")]
        public string WinnerName { get; set; }

        [DataMember(Name = "player1")]
        public string Player1 { get; set; }

        [DataMember(Name = "player2")]
        public string Player2 { get; set; }
    }

    public enum Player
    {
        Player1 = 0,
        Player2 = 1
    }

    public class CurrentScore
    {
        public string Name { get; set; }
        public int Wins { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        // MobileServiceCollectionView implements ICollectionView (useful for databinding to lists) and 
        // is integrated with your Mobile Service to make it easy to bind your data to the ListView
        private List<CurrentScore> leaderboard;

        private IMobileServiceTable<LeaderBoard> leaderboardTable = App.MobileService.GetTable<LeaderBoard>();

        private int[,] GameMatrix { get; set; }

        public Player CurrentPlayerIndex { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            GameMatrix = new int[3, 3];

        }

        public List<CurrentScore> LeaderBoard
        {
            get
            {
                return leaderboard;
            }
            set
            {
                leaderboard = value;
            }
        }

        private async void InsertLeaderBoardItem(LeaderBoard leaderBoardItem)
        {
            // This code inserts a new LeaderBoardItem into the database.
            await leaderboardTable.InsertAsync(leaderBoardItem);
        }



        private async void GetLeaderBoardItems()
        {
            IList<LeaderBoard> player1Wins = await leaderboardTable.Where(
                winner => winner.WinnerName == player1Name.Text).ToListAsync();
            IList<LeaderBoard> player2Wins = await leaderboardTable.Where(
                winner => winner.WinnerName == player2Name.Text).ToListAsync();
            leaderboard = new List<CurrentScore>();
            leaderboard.Add(new CurrentScore
            { 
                Name = (player1Wins.Count > player2Wins.Count? player1Name.Text:player2Name.Text),
                Wins = (player1Wins.Count > player2Wins.Count? player1Wins.Count:player2Wins.Count)
            });
            leaderboard.Add(new CurrentScore
            {
                Name = (player1Wins.Count > player2Wins.Count ? player2Name.Text : player1Name.Text),
                Wins = (player1Wins.Count > player2Wins.Count ? player2Wins.Count : player1Wins.Count)
            });
            ListItems.ItemsSource = leaderboard;
           
        }

        private void textBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetPlayerText(sender as TextBlock);
            int winner = CheckGameStatus();
            string winnerName = string.Empty;
            if (winner != -1)
            {
                winnerName = winner == (int)Player.Player1 ? player1Name.Text : player2Name.Text;
                winnerNameText.Text = winnerName + " Wins";
                InsertLeaderBoardItem(new LeaderBoard
                {
                    GameName = "TicTacToe",
                    GameStatus = 1,
                    Player1 = player1Name.Text,
                    Player2 = player2Name.Text,
                    WinnerName = winnerName
                });
                GetLeaderBoardItems();
            }
        }

        private void NewGameButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    GameMatrix[row, col] = 0;
                    TextBlock block = (TextBlock)this.FindName("textBlock" + row + "x" + col);
                    if (block != null)
                    {
                        block.Text = string.Empty;
                    }
                }
            }
            winnerNameText.Text = string.Empty;
            GetLeaderBoardItems();
        }

        #region Game Logic
        private int CheckGameStatus()
        {
            int rowStatus = CheckRows();
            if (rowStatus == -1)
            {
                rowStatus = CheckCols();
            }
            if (rowStatus == -1)
            {
                rowStatus = CheckDiagonal();
            }
            return rowStatus;
        }

        private void SetPlayerText(TextBlock currentTextBlock)
        {
            string textName = currentTextBlock.Name.Substring(currentTextBlock.Name.Length - 3, 3); ;
            string[] parts = textName.Split(new char[] { 'x' });
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);


            if (string.IsNullOrEmpty(currentTextBlock.Text))
            {
                if (CurrentPlayerIndex == Player.Player1)
                {
                    currentTextBlock.Text = "O";
                    CurrentPlayerIndex = Player.Player2;
                    GameMatrix[x, y] = 1;
                }
                else
                {
                    currentTextBlock.Text = "X";
                    CurrentPlayerIndex = Player.Player1;
                    GameMatrix[x, y] = 10;
                }
            }
        }

        int CheckRows()
        {
            for (int row = 0; row < 3; row++)
            {
                int rowValue = 0;
                for (int col = 0; col < 3; col++)
                {
                    rowValue += GameMatrix[row, col];
                }
                if (rowValue == 3)
                {
                    return (int)Player.Player1;
                }
                else if (rowValue == 30)
                {
                    return (int)Player.Player2;
                }
            }
            return -1;
        }

        int CheckCols()
        {
            for (int col = 0; col < 3; col++)
            {
                int colValue = 0;
                for (int row = 0; row < 3; row++)
                {
                    colValue += GameMatrix[row, col];
                }
                if (colValue == 3)
                {
                    return (int)Player.Player1;
                }
                else if (colValue == 30)
                {
                    return (int)Player.Player2;
                }
            }
            return -1;
        }

        int CheckDiagonal()
        {
            int diagValueF = 0;
            int diagValueB = 0;
            for (int positonF = 0, positonB = 2; positonF < 3; positonF++, positonB--)
            {
                diagValueF += GameMatrix[positonF, positonF];
                diagValueB += GameMatrix[positonF, positonB];
            }
            if (diagValueF == 3)
            {
                return (int)Player.Player1;
            }
            else if (diagValueF == 30)
            {
                return (int)Player.Player2;
            }
            if (diagValueB == 3)
            {
                return (int)Player.Player1;
            }
            else if (diagValueB == 30)
            {
                return (int)Player.Player2;
            }
            return -1;
        }
        #endregion
    }
}
