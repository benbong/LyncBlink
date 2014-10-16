using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Lync.Model;


namespace LyncBlink
{
    public partial class Form1 : Form
    {
        #region Fields
        // Current dispatcher reference for changes in the user interface.
        private Dispatcher dispatcher;
        private LyncClient lyncClient;
        #endregion

        public Form1()
        {
            InitializeComponent();

            //Save the current dispatcher to use it for changes in the user interface.
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Add the availability values to the ComboBox
            comboBox1.Items.Add(ContactAvailability.Free);
            comboBox1.Items.Add(ContactAvailability.Busy);
            comboBox1.Items.Add(ContactAvailability.DoNotDisturb);
            comboBox1.Items.Add(ContactAvailability.Away);

            //Listen for events of changes in the state of the client
            try
            {
                lyncClient = LyncClient.GetClient();
            }
            catch (ClientNotFoundException clientNotFoundException)
            {
                Console.WriteLine(clientNotFoundException);
                return;
            }
            catch (NotStartedByUserException notStartedByUserException)
            {
                Console.Out.WriteLine(notStartedByUserException);
                return;
            }
            catch (LyncClientException lyncClientException)
            {
                Console.Out.WriteLine(lyncClientException);
                return;
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                    return;
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }

            lyncClient.StateChanged +=
                new EventHandler<ClientStateChangedEventArgs>(Client_StateChanged);

        }

        /// <summary>
        /// Handler for the StateChanged event of the contact. Used to update the user interface with the new client state.
        /// </summary>
        private void Client_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            //Use the current dispatcher to update the user interface with the new client state.

        }

        private bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidCastException;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // N/A
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO
            // indicate which blink(1) you have 
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO
            // display the Licence file and some information
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void UpdateUserInterface(ClientState currentState)
        {

            if (currentState == ClientState.SignedIn)
            {
                //Listen for events of changes of the contact's information
                lyncClient.Self.Contact.ContactInformationChanged +=
                    new EventHandler<ContactInformationChangedEventArgs>(SelfContact_ContactInformationChanged);

                //Get the contact's information from Lync and update with it the corresponding elements of the user interface.
                SetAvailability();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please sign in!", "Lync not running", MessageBoxIcon.Exclamation);
            }
        }

        private void SetAvailability()
        {
            //Get the current availability value from Lync
            ContactAvailability currentAvailability = 0;
            try
            {
                currentAvailability = (ContactAvailability)
                                                          lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
            }
            catch (LyncClientException e)
            {
                Console.WriteLine(e);
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }


            if (currentAvailability != 0)
            {
                //Update the availability ComboBox with the contact's current availability.
                ComboBox1.SelectedValue = currentAvailability;

                //Choose a color to match the contact's current availability and update the border area containing the contact's photo
                Brush availabilityColor;
                switch (currentAvailability)
                {
                    case ContactAvailability.Away:
                        availabilityColor = Brushes.Yellow;
                        break;
                    case ContactAvailability.Busy:
                        availabilityColor = Brushes.Red;
                        break;
                    case ContactAvailability.BusyIdle:
                        availabilityColor = Brushes.Red;
                        break;
                    case ContactAvailability.DoNotDisturb:
                        availabilityColor = Brushes.DarkRed;
                        break;
                    case ContactAvailability.Free:
                        availabilityColor = Brushes.LimeGreen;
                        break;
                    case ContactAvailability.FreeIdle:
                        availabilityColor = Brushes.LimeGreen;
                        break;
                    case ContactAvailability.Offline:
                        availabilityColor = Brushes.LightSlateGray;
                        break;
                    case ContactAvailability.TemporarilyAway:
                        availabilityColor = Brushes.Yellow;
                        break;
                    default:
                        availabilityColor = Brushes.LightSlateGray;
                        break;
                }
                availabilityBorder.Background = availabilityColor;
            }
        }

    }
}
