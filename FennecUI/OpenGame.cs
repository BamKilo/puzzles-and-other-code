﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using POG.Forum;

namespace POG.FennecFox
{
    public partial class OpenGame : Form
    {
        LobbyReader _lobby;
        String _url;
        BindingList<ForumThread> _threads = new BindingList<ForumThread>();
        List<String> _gameIcons;
        String _OP;

        public OpenGame()
        {
            InitializeComponent();
        }
        public OpenGame(VBulletinForum forum, String lobbyURL, IEnumerable<String> gameIcons)
            : this()
        {
			_url = lobbyURL; 
            _lobby = forum.Lobby();
            _gameIcons = gameIcons.ToList();
        }
        public String GetURL(out String OP, out Boolean turbo)
        {
            turbo = chkTurbo.Checked;
            OP = _OP;
            String rc = txtURL.Text;
            return rc;
        }

        void _lobby_LobbyPageCompleteEvent(object sender, LobbyPageCompleteEventArgs e)
        {
            foreach (ForumThread t in e.Threads)
            {
                if ((_gameIcons.Count == 0) || _gameIcons.Contains(t.ThreadIconText))
                {
                    _threads.Add(t);
                }
            }
            lbThreads.ClearSelected();
        }

        private void lbThreads_SelectedIndexChanged(object sender, EventArgs e)
        {
            int ix = lbThreads.SelectedIndex;
            if (ix >= 0)
            {
                ForumThread t = lbThreads.Items[ix] as ForumThread;
                if (t != null)
                {
                    _OP = t.OP.Name;
                    txtURL.Text = t.URL;
                    if (t.ThreadIconText == "Club")
                    {
                        chkTurbo.Checked = true;
                    }
                    else
                    {
                        chkTurbo.Checked = false;
                    }
                }
            }
            else
            {
                txtURL.Text = String.Empty;
            }
        }

        private void OpenGame_Load(object sender, EventArgs e)
        {
            _lobby.LobbyPageCompleteEvent += new EventHandler<LobbyPageCompleteEventArgs>(_lobby_LobbyPageCompleteEvent);
            _lobby.ReadLobby(_url, 1, 1, true);
            lbThreads.DataSource = _threads;
            lbThreads.DisplayMember = "Title";
        }

        private void OpenGame_FormClosing(object sender, FormClosingEventArgs e)
        {
            _lobby.LobbyPageCompleteEvent -= _lobby_LobbyPageCompleteEvent;
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {
            if (txtURL.Text != String.Empty)
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }


    }
}
