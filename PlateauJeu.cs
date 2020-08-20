﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace BookWorm
{
    public partial class PlateauJeu : Form
    {
        string motBonus;
        // Récupérer le dictionnaire des mots
        string[] wordList = Util.ReadTextFile(Constant.MAIN_WORD_LIST_FILE_PATH);
        int[] topRowButtonsIndexListe = { 6, 14, 21, 29, 36, 44, 51};
        int[] bottomRowButtonsIndexListe = { 45, 37, 30, 22, 15, 7, 0 };
        public static ArrayList caseLettreEnFeu = new ArrayList();
        System.Timers.Timer chrono;

        public PlateauJeu()
        {
            InitializeComponent();
        }

        private void PlateauJeu_Load(object sender, EventArgs e)
        {
            // Init le mot courant
            currentWordLabel.Text = "";

            // Init le mot bonus :
            var motBonusListe = Util.ReadTextFile(Constant.MOT_BONUS_FILE_PATH);
            Random random = new Random();
            motBonus = motBonusListe[random.Next(0,motBonusListe.Length)];
            bonusWordLabel.Text = motBonus;

            // Générer un nouveau plateau
            GenererNouveauPlateau();

            foreach (Button btn in plateauLettres.Controls.OfType<Button>())
            {
                // Brancher les événements pour le Drag and Drop
                btn.AllowDrop = true;
                btn.MouseDown += new MouseEventHandler(button1_MouseDown);
                btn.DragEnter += new DragEventHandler(button1_DragEnter);
                btn.DragDrop += new DragEventHandler(button1_DragDrop);

                // Init l'état des buttons
                btn.BackColor = SystemColors.ButtonFace; // Default color
                btn.Tag = Constant.NOT_SELECTED; // Etat = non sélectionné
            }
        }

        struct nbOccurLettres
        {
            // Structure de donnée pour garder le compte du nombre d'occurence actuel de chaque lettre dans le tableau afin de s'arranger pour sortir plus au moins les lettres selon leurs fréquences d'utilisationrs dans la langue française
            public char lettre;
            public int nbOccurMax;
            public int nbOccurPlateau;
        }

        nbOccurLettres[] structTest = new nbOccurLettres[26];
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        Random random = new Random();

        private string GenererNouvelleLettre()
        {
            int randConst = 0;
            bool relancerRandom = true;
            

            string tempLettre;


            while (relancerRandom)
            {
                // random pour obtenir un int qui va correspondre à une lettre de l'alphabet
                randConst = random.Next(0, Constant.ALPHABET_SANS_ACCENT.Length);

                // test si la lettre tirée a déjà excédé son quota d'apparition
                if (structTest[randConst].nbOccurPlateau < structTest[randConst].nbOccurMax)
                {

                    // si elle ne l'a pas excédé, on incrémente le compteur d'apparition de cette lettre et on sort de la boucle
                    structTest[randConst].nbOccurPlateau++;

                    relancerRandom = false;
                }
            }

            // on récupère la lettre qui correspond à ce numéro dans l'alphabet
            tempLettre = Constant.ALPHABET_SANS_ACCENT.ToCharArray()[randConst].ToString();

            return tempLettre;
        }

        private void GenererNouveauPlateau()
        {
            // array qui contient les quotas d'apparition de chaque lettre
            int[] maxOccurLettres = {5, 1, 2, 2, 9, 1, 1, 1, 4, 1, 1, 3, 2, 4, 3, 2, 1, 4, 5, 4, 4, 1, 1, 1, 1, 1};
            
            // Initialisation de la variable structurée
            for (int i = 0; i < 26; i++)
            {
                structTest[i].lettre = alphabet[i];
                structTest[i].nbOccurMax = maxOccurLettres[i];
                structTest[i].nbOccurPlateau = 0;
            }


            foreach (Button caseLettre in plateauLettres.Controls.OfType<Button>())
            {

                // on remplace le texte du bouton par la lettre
                caseLettre.Text = GenererNouvelleLettre();

                caseLettre.Font = new Font(Font.FontFamily.Name, 20);
            }

            
        }

        string letter;
        int index_btn_depart;
        int posX_btnDepart;
        int posY_btnDepart;
        MouseEventArgs evnt;

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;

            letter = btn.Text;
            index_btn_depart = plateauLettres.Controls.IndexOf(btn);

            // on stock la position du bouton de départ et on y rajoute 44 pour l'abcisse et l'ordonnée pour recentrer le marqueur de position dans l'axe
            posX_btnDepart = btn.Left + 44;
            posY_btnDepart = btn.Top + 44;

            // Si la lettre n'est pas encore sélectionnée
            // => Changer son état à SELECTED, sa couleur et faire le Drag&Drop
            // Uniquement pour la toute première lettre cliquée
            if (btn.Tag.ToString() == Constant.NOT_SELECTED)
            {
                currentWordLabel.Text += btn.Text;
                btn.BackColor = Color.BurlyWood;
                btn.Tag = Constant.SELECTED;
                evnt = e;

                btn.DoDragDrop("bidon", DragDropEffects.Link);
            }
        }

        private void button1_DragDrop(object sender, DragEventArgs e)
        {
            /*
             * Quand le joueur relâche le souris
             */

            // Vérifier si le mot sélectionné existe dans le dictionnaire
            CheckMatchingWord(currentWordLabel.Text);

            // Réinitialiser le plateau après que le souris est relâché
            currentWordLabel.Text = ""; // Réinit le mot courant
            foreach (Button btn in plateauLettres.Controls.OfType<Button>())
            {
                // Réinit l'état des boutons
                btn.BackColor = SystemColors.ButtonFace; // Default color
                btn.Tag = Constant.NOT_SELECTED; // Etat = non sélectionné
            }
        }

        private void button1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;

            Button btn = (Button)sender;

            int posX_btnArrivee = btn.Left + 44;
            int posY_btnArrivee = btn.Top + 44;

            // Vérifier que ce n'est pas la même lettre
            if (plateauLettres.Controls.IndexOf(btn) != index_btn_depart)
            {
                // Vérifier :
                // => si le btnArrivee est le voisin du btnDepart
                // => si le btnArrivee n'est pas sélectionné
                if (posX_btnArrivee - posX_btnDepart <= 100 && posX_btnArrivee - posX_btnDepart >= -100 && posY_btnArrivee - posY_btnDepart <= 100 && posY_btnArrivee - posY_btnDepart >= -100 && btn.Tag.ToString() == Constant.NOT_SELECTED)
                {
                    // Ajouter la lettre à la fin du mot courant
                    currentWordLabel.Text += btn.Text;

                    // Changer l'état du bouton à SELECTED
                    btn.BackColor = Color.BurlyWood;
                    btn.Tag = Constant.SELECTED;

                    // Maintenant le btnArrivee sera le btnDepart
                    button1_MouseDown(btn, evnt);
                }
            }
        }

        private void CheckMatchingWord(string mot)
        {
            mot = mot.ToLower();
            var isMatch = Array.Exists(wordList, s => s.Equals(mot));

            if(isMatch)
            {
                MessageBox.Show("Ok", "OK");
                ReplaceCharacters();
            }
        }

        private void ReplaceCharacters()
        {

            var btnArray = plateauLettres.Controls.OfType<Button>();
            for (int k = btnArray.Count() -1; k >= 0; k--)
            {
                Button btn = btnArray.ElementAt(k);

                if (btn.Tag.ToString() == Constant.SELECTED)
                {
                    bool topRowReached = false;
                    int i = plateauLettres.Controls.IndexOf(btn);

                    while(!topRowReached)
                    {
                        if(topRowButtonsIndexListe.Contains(i))
                        {
                            // générer une nouvelle lettre
                            plateauLettres.Controls[i].Text = GenererNouvelleLettre();

                            topRowReached = true;
                        }
                        else
                        {
                            plateauLettres.Controls[i].Text = plateauLettres.Controls[i + 1].Text;
                        }

                        i++;
                    }
                }
            }
        }

        private void mixWordBoardButton_Click(object sender, EventArgs e)
        {
            GenererNouveauPlateau();

            bool nouvelleCaseEnFeu = true;
            int randConst = 0;

            while (nouvelleCaseEnFeu)
            {
                // tirage au sort d'une case qui va être en feu
                randConst = random.Next(0, Constant.ALPHABET_SANS_ACCENT.Length);

                if (plateauLettres.Controls[randConst].BackColor != Color.Red)
                {
                    nouvelleCaseEnFeu = false;
                }
            }

            caseLettreEnFeu.Add(randConst);

            plateauLettres.Controls[randConst].BackColor = Color.Red;

            // Init du chrono pour faire descendre les cases en feu toutes les 2000 milliscondes
            chrono = new System.Timers.Timer();
            chrono.Interval = 2000;
            chrono.Elapsed += OnTimedEvent;
            chrono.AutoReset = true;
            chrono.Enabled = true;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // parcourir l'array qui contient l'index de toutes les cases en feu
            for (int i = 0; i < caseLettreEnFeu.Count; i++)
            {
                // verification qu'on a pas atteint la première case pour éviter d'atteindre l'index -1
                if ((int)caseLettreEnFeu[i] - 1 > 0)
                {
                    // on remet la couleur du bouton par défaut
                    plateauLettres.Controls[(int)caseLettreEnFeu[i]].BackColor = Color.WhiteSmoke;

                    // on décrémente de 1 l'index des cases en feu contenues dans l'array "caseLettreEnFeu"
                    caseLettreEnFeu[i] = (int)caseLettreEnFeu[i] - 1;

                    // on colorie en rouge le bouton d'index -1 (on fait descendre la case en feu)
                    plateauLettres.Controls[(int)caseLettreEnFeu[i]].BackColor = Color.Red;
                }
                
                // Vérification si une case en feu qui descend atteint le bas du tableau (défaite)
                if (bottomRowButtonsIndexListe.Contains((int)caseLettreEnFeu[i]))
                {
                    // désactivation du chronomètre car fin de la partie
                    chrono.Enabled = false;

                    MessageBox.Show("Perdu !");
                }
                
            }
        }

        private void RevenirAuMenu(object sender, EventArgs e)
        {
            //if (chrono.Enabled = true)
            {
                //chrono.Enabled = false;
            }
            

            this.Close();
        }

    }
}
