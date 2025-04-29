using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace gps_package_tester
{
    public partial class Form1 : Form
    {
        private int[] package_types = new int[5];

        private decimal total_score = 0;

        private PictureBox[] packages_picture = new PictureBox[5];
        private Label[] packages_label = new Label[5];
        private CheckBox[] packages_contaminated = new CheckBox[5];
        private CheckBox[] packages_random = new CheckBox[5];
        private NumericUpDown[] b_grade_brakets = new NumericUpDown[6];

        private List<decimal> scores = new List<decimal>();
        public Form1()
        {
            InitializeComponent();
            AddPackageComponents();
        }

        private void AddPackageComponents()
        {
            packages_picture[0] = b_package_0;
            packages_picture[1] = b_package_1;
            packages_picture[2] = b_package_2;
            packages_picture[3] = b_package_3;
            packages_picture[4] = b_package_4;

            packages_label[0] = package_0_label;
            packages_label[1] = package_1_label;
            packages_label[2] = package_2_label;
            packages_label[3] = package_3_label;
            packages_label[4] = package_4_label;

            packages_contaminated[0] = b_contaminated_0;
            packages_contaminated[1] = b_contaminated_1;
            packages_contaminated[2] = b_contaminated_2;
            packages_contaminated[3] = b_contaminated_3;
            packages_contaminated[4] = b_contaminated_4;

            packages_random[0] = b_random_0;
            packages_random[1] = b_random_1;
            packages_random[2] = b_random_2;
            packages_random[3] = b_random_3;
            packages_random[4] = b_random_4;

            b_grade_brakets[0] = b_grade_f_min;
            b_grade_brakets[1] = b_grade_d_min;
            b_grade_brakets[2] = b_grade_c_min;
            b_grade_brakets[3] = b_grade_b_min;
            b_grade_brakets[4] = b_grade_a_min;
            b_grade_brakets[5] = b_grade_s_min;
        }

        private void b_package_0_Click(object sender, EventArgs e)
        {
            UpdatePackageState(0);
        }

        private void b_package_1_Click(object sender, EventArgs e)
        {
            UpdatePackageState(1);
        }

        private void b_package_2_Click(object sender, EventArgs e)
        {
            UpdatePackageState(2);
        }

        private void b_package_3_Click(object sender, EventArgs e)
        {
            UpdatePackageState(3);
        }

        private void b_package_4_Click(object sender, EventArgs e)
        {
            UpdatePackageState(4);
        }

        private void UpdatePackageState(int package_index, bool increment = true)
        {
            Image pack_image = Properties.Resources.txt_package_random;

            package_types[package_index]++;
            if (package_types[package_index] > 3) package_types[package_index] = 0;

            switch (package_types[package_index])
            {
                case 1:
                    pack_image = Properties.Resources.txt_package_correct;
                    break;
                case 2:
                    pack_image = Properties.Resources.txt_package_incorrect;
                    break;
                case 3:
                    pack_image = Properties.Resources.txt_package_timeout;
                    break;
            }

            packages_picture[package_index].Image = pack_image;
        }

        private void b_generate_Click(object sender, EventArgs e)
        {
            Generate();
        }

        private void Generate()
        {
            total_score = 0;
            int multiply_index = 0;
            int p_type = 0;
            int total_time = 0;
            int contaminated_boxes = 0;
            decimal max_score = 0;
            max_score += (b_correct_amount.Value + b_package_time.Value) * 5;
           
            for (int i = 0; i < 5; i++)
            {
                max_score += b_chain_bonus.Value * i;

                Random rnd = new Random();
               
                if (package_types[i] == 0 || packages_random[i].Checked)
                {
                    package_types[i] = rnd.Next(0, 3);
                    UpdatePackageState(i, false);
                }

                if (i == 0) p_type = package_types[0];

                decimal package_points = 0;
                decimal chain_points = 0;

                int decom_bonus = 0;
                int time_taken = (int)b_avg_time.Value + rnd.Next(-((int)b_avg_time.Value / 2), ((int)b_avg_time.Value / 2));
                bool contaminated = packages_contaminated[i].Checked;

                if (contaminated) contaminated_boxes++;

                if (time_taken > b_package_time.Value && package_types[i] == 1) time_taken = (int)b_package_time.Value - 1;
                if (time_taken < b_package_time.Value && package_types[i] == 3) time_taken = (int)b_package_time.Value + 1;

                total_time += time_taken;

                if (contaminated && rnd.Next(0, 100) < b_decontaminate_chance.Value) decom_bonus += (int)b_decom_bonus.Value;

                switch (package_types[i])
                {
                    case 1: // correct
                        chain_points = b_chain_bonus.Value * multiply_index;
                        package_points += b_correct_amount.Value + (b_package_time.Value - time_taken);
                        multiply_index++;
                        break;

                    case 2: // incorrect

                        package_points = -(time_taken + b_incorrect_penalty.Value);

                        multiply_index = 0;
                        decom_bonus = 0;
                        break;

                    case 3: // timeout

                        package_points += (b_correct_amount.Value + (b_package_time.Value - time_taken)) / b_timeout_division.Value;
                        multiply_index = 0;
                        break;
                }
                total_score += package_points + chain_points;
                total_score += decom_bonus;

                packages_label[i].Text = "Index: " + package_types[i] + "\nPoints added: " + (int)package_points + "\n" + "Chain index: " + multiply_index + "\nChain points: " + chain_points + "\nDecomtaminate Points: " + decom_bonus + "\nTime: " + time_taken + "s";
            }

            max_score += b_decom_bonus.Value * contaminated_boxes;
            l_max_score.Text = "Max score: " + max_score;
            time_total_label.Text = "Total Time: " + total_time + "s";

            scores.Add(total_score);
            if (scores.Count > 256) scores.RemoveAt(1);

            decimal total = 0;
            for (int i = 0; i < scores.Count; i++) total += scores[i];

            int average = (int)(total / scores.Count);

            grade_label.Text = "Grade: " + GetGrade(total_score) + ", (Total score: " + (int)total_score + "/" + max_score + ") Average: " + GetGrade(average) + ", " + (int)average + " (" + scores.Count + ")";

            if (total_score < 0) total_score = 0;
            if (total_score > max_score) total_score = max_score;

            grade_bar.Maximum = (int)max_score;
            grade_bar.Value = (int)total_score;
        }

        private string GetGrade(decimal score)
        {
            string[] grades = new string[6] { "F", "D", "C", "B", "A", "S" };
            int grade = 0;

            for (int i = 0; i < 6; i++)
            {
                if (score > b_grade_brakets[i].Value)
                {
                    grade = i;
                }
                else break;
            }

            return grades[grade];
        }

        private void LoadData()
        {
            string[] data_string = data_box.Text.Split(',');

            bool found_preset = data_box.Text != null && data_box.Text.Length > 0;
            if (data_string.Length < 14) found_preset = false;

            grade_label.Text = found_preset ? "Loaded preset data." : "Data string invalid, missing entries.";
            if (!found_preset) return;

            decimal[] data_set = new decimal[data_string.Length];

            for (int i = 0; i < data_string.Length; i++)
            {
                if (!decimal.TryParse(data_string[i], out data_set[i]))
                {
                    grade_label.Text = "Data string invalid, incorrect data types.";
                    return;
                }
            }

            b_grade_s_min.Value = data_set[0];
            b_grade_a_min.Value = data_set[1];
            b_grade_b_min.Value = data_set[2];
            b_grade_c_min.Value = data_set[3];
            b_grade_d_min.Value = data_set[4];
            b_grade_f_min.Value = data_set[5];

            b_package_time.Value = data_set[6];
            b_decom_bonus.Value = data_set[7];
            b_correct_amount.Value = data_set[8];
            b_incorrect_penalty.Value = data_set[9];
            b_timeout_division.Value = data_set[10];
            b_chain_bonus.Value = data_set[11];

            b_avg_time.Value = data_set[12];
            b_decontaminate_chance.Value = data_set[13];
        }

        private void b_loaddata_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void b_reset_average_Click(object sender, EventArgs e)
        {
            scores.Clear();
            grade_label.Text = "Reset average score.";
        }

        private void b_auto_gen_CheckedChanged(object sender, EventArgs e)
        {
            auto_timer.Enabled = b_auto_gen.Checked;
        }

        private void auto_timer_Tick(object sender, EventArgs e)
        {
            Generate();
        }

        private void b_convert_Click(object sender, EventArgs e)
        {
            grade_label.Text = "Converting to preset string.";

            decimal[] data_set = new decimal[14];

            data_set[0] = b_grade_s_min.Value;
            data_set[1] = b_grade_a_min.Value;
            data_set[2] = b_grade_b_min.Value;
            data_set[3] = b_grade_c_min.Value;
            data_set[4] = b_grade_d_min.Value;
            data_set[5] = b_grade_f_min.Value;

            data_set[6] = b_package_time.Value;
            data_set[7] = b_decom_bonus.Value;
            data_set[8] = b_correct_amount.Value;
            data_set[9] = b_incorrect_penalty.Value;
            data_set[10] = b_timeout_division.Value;
            data_set[11] = b_chain_bonus.Value;

            data_set[12] = b_avg_time.Value;
            data_set[13] = b_decontaminate_chance.Value;

            string data_string = "";

            for (int i = 0; i < data_set.Length; i++)
            {
                data_string += data_set[i];
                if (i < data_set.Length - 1) data_string += ",";
            }

            data_box.Text = data_string;
        }

        private void b_random_all_Click(object sender, EventArgs e)
        {
            bool random = false;
            for (int i = 0; i < packages_random.Length; i++)
            {
                if (i == 0) random = !packages_random[0].Checked;
                packages_random[i].Checked = random;
            }
        }
    }
}
