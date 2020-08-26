﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookWorm
{
    public partial class NomJoueur : Form
    {
        public NomJoueur()
        {
            InitializeComponent();
        }

        private void ConfirmPlayerNameButtonClick(object sender, EventArgs e)
        {
            if (playerNameTextBox.Text == "")
            {
                MessageBox.Show("Veuillez saisir votre nom", "Impossible de continuer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Util.nomJoueur = playerNameTextBox.Text;
                Util.PlayButtonClickSound();
                LancerPlateauJeu();
            }
        }

        private void LancerPlateauJeu()
        {
            Form nouveauPlateauJeu = new PlateauJeu();
            nouveauPlateauJeu.Location = this.Location;
            nouveauPlateauJeu.StartPosition = FormStartPosition.Manual;
            nouveauPlateauJeu.FormClosing += delegate
            {
                this.Location = nouveauPlateauJeu.Location;
                this.Close();
            };
            Util.backgroundMusic.Pause();
            nouveauPlateauJeu.Show();
            this.Hide();
        }
    }
}