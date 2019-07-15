// Copyright (c) Microsoft Corporation.  All rights reserved. 


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.PersistentChat;
using Microsoft.Rtc.Collaboration.PersistentChat.Management;
using Microsoft.Rtc.Signaling;

namespace PersistentChatLibrarySamples
{
    /// <summary>
    /// Simulates a multiple-user load on the server.  Assumes the users have been created and SIP-enabled.
    /// Creates the necessary category and chat rooms if they are missing.
    /// </summary>
    static class SampleLoadTest
    {

        public   static  Uri roomUri ;
         public static List<SimulatedClient> clients = new List<SimulatedClient>();
        public static  void getClient(){

            // Generate the clients and the user uris that they'll use to authenticate with Lync Server.
            // These users must already exist in the test environment and be SIP-enabled.
          
            for (int i = 2; i <= 5; i++)
            {
                SimulatedClient client = new SimulatedClient(SampleCommon.GetLoadTestUserUri(i),
                    SampleCommon.GetLoadTestUserName(i), SampleCommon.GetLoadTestUserPassword(i));

                Console.WriteLine("getClient:" + client.UserUri + "  userName:" + client.UserName + " client:" + client.Password);
                clients.Add(client);
            }
            Console.WriteLine("Press any key to start the test,  已经加载的用户");
              Console.ReadKey();
            // Create new chat rooms.
            List<Uri> chatRooms = SetupChatRooms(clients);

            // Let user control when the test run starts.
            Console.WriteLine();
            Console.WriteLine("Press any key to start the test, then press any key later to longin stop the test...");
            Console.ReadKey();

            // Start the connection process for each client.  Once connected, each client will do work
            // (join chat rooms, chat, etc.) until they are told to Stop().
            foreach (SimulatedClient client in clients)
            {
                Console.WriteLine("正在登陆:" + client.UserUri + "  userName:" + client.UserName + " client:" + client.Password);
                // NOTE: Could wait a random delay here between each login.
              //  client.Start(chatRooms);
                Console.ReadKey();
                Console.WriteLine("登陆end:" + client.UserUri + "  userName:" + client.UserName + " client:" + client.Password);
            }

           
          

           
    
    }

        public static void stopClient(){
            // Stop all the objects and cleanup.
            foreach (SimulatedClient client in clients)
            {
                client.Stop();
            }
        } 

        /// <summary>
        /// Create the category and chat rooms if they are missing, or simply find their Uris if they
        /// exist already (presumably from a previous run of this sample code).
        /// </summary>
        /// <param name="clients"></param>
        /// <returns></returns>
        private static List<Uri> SetupChatRooms(IEnumerable<SimulatedClient> clients)
        {
            List<Uri> chatRooms = new List<Uri>();

            try
            {
                UserEndpoint userEndpoint = SampleCommon.ConnectLyncServer(SampleCommon.UserSipUri,
                                                                           SampleCommon.LyncServer,
                                                                           SampleCommon.UsingSso,
                                                                           SampleCommon.Username,
                                                                           SampleCommon.Password);
                PersistentChatEndpoint persistentChatEndpoint = SampleCommon.ConnectPersistentChatServer(userEndpoint,
                                                                                          SampleCommon.
                                                                                              PersistentChatServerUri);

                Console.WriteLine("Press any key to start the test, then press any key later to longin  the test...");
                Console.ReadKey();
                foreach (string roomName in SampleCommon.LoadTestChatRoomNames)
                {
                    
                    chatRooms.Add(roomUri);

                    // Setup all the test users as Managers and Members in this chat room
                    // thus allowing these users to join the chat rooms during this sample's execution.
                    foreach (SimulatedClient client in clients)
                    {
                        ChatRoomAddManagerAndMember(persistentChatEndpoint, roomUri, client.UserUri);
                    }
                }

                SampleCommon.DisconnectPersistentChatServer(persistentChatEndpoint);
                SampleCommon.DisconnectLyncServer(userEndpoint);
            }
            catch (InvalidOperationException invalidOperationException)
            {
                Console.Out.WriteLine("InvalidOperationException: " + invalidOperationException.Message);
            }
            catch (ArgumentNullException argumentNullException)
            {
                Console.Out.WriteLine("ArgumentNullException: " + argumentNullException.Message);
            }
            catch (ArgumentException argumentException)
            {
                Console.Out.WriteLine("ArgumentException: " + argumentException.Message);
            }
            catch (Microsoft.Rtc.Signaling.AuthenticationException authenticationException)
            {
                Console.Out.WriteLine("AuthenticationException: " + authenticationException.Message);
            }
            catch (Microsoft.Rtc.Signaling.FailureResponseException failureResponseException)
            {
                Console.Out.WriteLine("FailureResponseException: " + failureResponseException.Message);
            }
            catch (UriFormatException uriFormatException)
            {
                Console.Out.WriteLine("UriFormatException: " + uriFormatException.Message);
            }
            catch (Exception exception)
            {
                Console.Out.WriteLine("Exception: " + exception.Message);
            }
            return chatRooms;
        }

        private static void ChatRoomAddManagerAndMember(PersistentChatEndpoint persistentChatEndpoint, Uri chatroomUri, Uri userUri)
        {

            ChatRoomManagementServices chatroomMgmt = persistentChatEndpoint.PersistentChatServices.ChatRoomManagementServices;
            PersistentChatUserServices userMgmt = persistentChatEndpoint.PersistentChatServices.UserServices;

            PersistentChatUser user = userMgmt.EndGetUser(userMgmt.BeginGetUser(userUri, null, null));
            List<PersistentChatPrincipalSummary> newUsers = new List<PersistentChatPrincipalSummary> { user };

            chatroomMgmt.EndAddUsersOrGroupsToRole((chatroomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Manager, chatroomUri, newUsers, null, null)));
            chatroomMgmt.EndAddUsersOrGroupsToRole((chatroomMgmt.BeginAddUsersOrGroupsToRole(ChatRoomRole.Member, chatroomUri, newUsers, null, null)));

        }

    

        private enum State
        {
            Started,
            Stopping,
            Stopped
        }

        /// <summary>
        /// Represents one instance of a Persistent Chat Client.  Logs in, downloads preferences from 
        /// the Persistent Chat Server, joins multiple chat rooms, etc.
        /// </summary>
        internal class SimulatedClient
        {
            // Data that's passed in
            public Uri UserUri { get; private set; }
            public string UserName { get; private set; }
            public string Password { get; private set; }
            private List<Uri> chatRooms;

            // Data that's generated along the way
            private readonly CurrentOperation currentOperation;
            private UserEndpoint userEndpoint;
            private PersistentChatEndpoint persistentChatEndpoint;
            private readonly List<SimulatedChatRoom> chatRoomSessions = new List<SimulatedChatRoom>();

            public SimulatedClient(Uri userUri, string userName, string password)
            {
                UserUri = userUri;
                UserName = userName;
                Password = password;
            }

            public void Start(List<Uri> chatRoomsToJoin)
            {

                Console.WriteLine("正在登陆用户");
                Console.ReadKey();

                try
                {
                    chatRooms = chatRoomsToJoin;

                    // Connect to Lync Server
                    ClientPlatformSettings platformSettings = new ClientPlatformSettings("PersistentChat.Test", SipTransportType.Tls);
                    CollaborationPlatform collabPlatform = new CollaborationPlatform(platformSettings);
                    collabPlatform.EndStartup(collabPlatform.BeginStartup(null, null));
                    UserEndpointSettings userEndpointSettings = new UserEndpointSettings(UserUri.AbsoluteUri, SampleCommon.LyncServer);
                    userEndpointSettings.Credential = new NetworkCredential(UserName, Password);
                    userEndpoint = new UserEndpoint(collabPlatform, userEndpointSettings);
                    Console.WriteLine("正在登陆用户----------->");
                    // Login to Lync Server
                    currentOperation.Begin("Connect to Lync Server",
                                           () => userEndpoint.BeginEstablish(ar => LyncServerBeginEstablishFinished(ar), null));
                }
                catch (Exception e) {
                    Console.WriteLine("正在登陆用户" );
                }
            }

            public void Stop()
            {
                currentOperation.Stop(
                    () =>
                    {
                        // Wait for rooms to stop doing work
                        List<WaitHandle> waitHandles = new List<WaitHandle>();
                        foreach (SimulatedChatRoom room in chatRoomSessions)
                        {
                            waitHandles.Add(room.WaitForStopped);
                            room.Stop();
                        }
                        if(waitHandles.Count>0)
                            WaitHandle.WaitAll(waitHandles.ToArray());
                    },
                    () =>
                    {
                        // Teardown
                        chatRoomSessions.Clear();
                        chatRooms.Clear();

                        if (persistentChatEndpoint != null)
                        {
                            SampleCommon.DisconnectPersistentChatServer(persistentChatEndpoint);
                            persistentChatEndpoint = null;
                        }
                        if (userEndpoint != null)
                        {
                            SampleCommon.DisconnectLyncServer(userEndpoint);
                            userEndpoint = null;
                        }
                    });
            }

          
            private void LyncServerBeginEstablishFinished(IAsyncResult ar)
            {
                currentOperation.End("LyncServerBeginEstablishFinished", () => userEndpoint.EndEstablish(ar));

                currentOperation.Begin(string.Format("Connect to GC={0}", SampleCommon.PersistentChatServerUri),
                    () =>
                        {
                            // Connect to Persistent Chat Server
                            persistentChatEndpoint = new PersistentChatEndpoint(SampleCommon.PersistentChatServerUri, userEndpoint);
                            persistentChatEndpoint.BeginEstablish(ar1 => PersistentChatBeginEstablishFinished(ar1), null);
                        });
            }

            private void PersistentChatBeginEstablishFinished(IAsyncResult ar)
            {
                currentOperation.End("PersistentChatBeginEstablishFinished", () => persistentChatEndpoint.EndEstablish(ar));

                // Persistent Chat Client actually requests 11 different bundles of preferences
                currentOperation.Begin("Get Client Preferences", () => persistentChatEndpoint.PersistentChatServices.
                                                                           BeginGetPreferenceBundle("PrefLabel", -1,
                                                                                                    false,
                                                                                                    BeginGetPreferenceBundleFinished,
                                                                                                    null));
            }

            private void BeginGetPreferenceBundleFinished(IAsyncResult ar)
            {
                PersistentChatPreferenceBundle prefBundle;
                currentOperation.End("BeginGetPreferenceBundleFinished", () => prefBundle = persistentChatEndpoint.PersistentChatServices.EndGetPreferenceBundle(ar));

                // Persistent Chat Client stores the lastInviteID in one of the prefs bundles, and it would provide that # here
                // instead of "0".  This would tell the server which Chat Room invitations we've already seen so the server
                // could avoid sending them again.
                currentOperation.Begin("Get Chat Room Invitations",
                                       () =>
                                       persistentChatEndpoint.PersistentChatServices.BeginBrowseChatRoomsByInvitations(0,
                                                                                                             BeginBrowseChatRoomsByInvitationsFinished,
                                                                                                             null));
            }

            private void BeginBrowseChatRoomsByInvitationsFinished(IAsyncResult ar)
            {
                ReadOnlyCollection<ChatRoomInvitation> chatRoomInvitations;
                int lastInviteID;
                currentOperation.End("BeginBrowseChatRoomsByInvitationsFinished", () => chatRoomInvitations = persistentChatEndpoint.PersistentChatServices.EndBrowseChatRoomsByInvitations(ar, out lastInviteID));

                // Persistent Chat Client stores which Chat Rooms we should automatically join in one of the prefs bundles, and
                // joins them at this point.
                BeginJoinChatRooms();
            }

            private void BeginJoinChatRooms()
            {
                chatRoomSessions.Clear();
                foreach (Uri roomUri in chatRooms)
                {
                    if (!currentOperation.IsRunning) return;

                    SimulatedChatRoom chatRoom = new SimulatedChatRoom(persistentChatEndpoint, this, roomUri);
                    chatRoomSessions.Add(chatRoom);
                    chatRoom.Start();
                }
            }
        }

        /// <summary>
        /// Represents one of the multiple chat rooms that a SimulatedClient is joining, chatting in, 
        /// leaving, etc.
        /// </summary>
        internal class SimulatedChatRoom
        {
            // Data that's passed in
            private SimulatedClient Client { get; set; }
            private readonly Uri roomUri;

            // Data that's generated along the way
            private readonly CurrentOperation currentOperation;
            private readonly ChatRoomSession session;
            private int sentChatMessages;

            public SimulatedChatRoom(PersistentChatEndpoint persistentChatEndpoint, SimulatedClient client, Uri roomUri)
            {
                Client = client;
                this.roomUri = roomUri;
                session = new ChatRoomSession(persistentChatEndpoint);
            }

            public void Start()
            {
                currentOperation.Reset();

                currentOperation.Begin("Join ChatRoom:", () => session.BeginJoin(roomUri, BeginJoinChatRoomFinished, null));
            }

            public void Stop()
            {
                currentOperation.Stop(null, null);
            }

            public WaitHandle WaitForStopped {get { return ((IStoppable)currentOperation).WaitForStopped; }}

          
            private void BeginJoinChatRoomFinished(IAsyncResult ar)
            {
                currentOperation.End("BeginJoinChatRoomFinished", () => session.EndJoin(ar));
                currentOperation.Begin("Get Recent History:", () => session.BeginGetRecentChatHistory(30, BeginGetRecentChatHistoryFinished, null));
            }

            private void BeginGetRecentChatHistoryFinished(IAsyncResult ar)
            {
                ReadOnlyCollection<ChatMessage> history;
                currentOperation.End("BeginGetRecentChatHistoryFinished", () => history = session.EndGetRecentChatHistory(ar));

                // NOTE: Could do something with history

                sentChatMessages = 1;
                string label = string.Format("Send Msg: # {0}", sentChatMessages);
                currentOperation.Begin(label, () => session.BeginSendChatMessage("simple message from " + Client.UserName, false,
                                                   BeginSendChatMessageFinished, null));
            }

            private void BeginSendChatMessageFinished(IAsyncResult ar)
            {
                currentOperation.End("BeginSendChatMessageFinished", () => session.EndSendChatMessage(ar));

                if (sentChatMessages < 10)
                {
                    sentChatMessages++;
                    currentOperation.Begin(string.Format("Send Msg: # {0}", sentChatMessages), 
                        () => session.BeginSendChatMessage("simple message from " + Client.UserName, false,
                                                 BeginSendChatMessageFinished, null));
                }
                else
                {
                    // After sending 10 chat msgs, leave the chat room (and rejoin).
                    currentOperation.Begin("Leave ChatRoom:", () => session.BeginLeave(BeginLeaveChatRoomFinished, null));
                }
            }

            private void BeginLeaveChatRoomFinished(IAsyncResult ar)
            {
                currentOperation.End("BeginLeaveChatRoomFinished", () => session.EndLeave(ar));

                // After leaving the chat room, join it again.
                currentOperation.Begin("RE-Join ChatRoom:", () => session.BeginJoin(roomUri, BeginJoinChatRoomFinished, null));
            }
        }

        /// <summary>
        /// Mechanism that can be used to wait patiently for pending operations to finish after the
        /// user tells the sample to Stop.
        /// </summary>
        private class CurrentOperation : IStoppable
        {
            private readonly Action<string> logMethod;
            private readonly ManualResetEvent currentOperationFinished = new ManualResetEvent(false);
            private bool currentOperationStarted;
            private State state = State.Stopped;
            private readonly object stateChangeLock = new object();
            private readonly ManualResetEvent waitForStopped = new ManualResetEvent(false);

            public CurrentOperation(Action<string> logMethod)
            {
                this.logMethod = logMethod;
            }

            public void Reset()
            {
                lock (stateChangeLock)
                {
                    state = State.Started;
                }
                lock(currentOperationFinished)
                {
                    currentOperationFinished.Reset();
                    currentOperationStarted = false;
                    logMethod("Reset cleared currentOperationStarted");
                }
                waitForStopped.Reset();
            }

            public bool Begin(string label, VoidDelegate action)
            {
                lock (stateChangeLock)
                {
                    if (state != State.Started) return false;

                    logMethod(label);
                    lock (currentOperationFinished)
                    {
                        currentOperationFinished.Reset();
                        currentOperationStarted = true;
                        logMethod("Begin set currentOperationStarted: " + label);
                    }
                    action();
                }
                return true;
            }

            public void End(string label, VoidDelegate action)
            {
                try
                {
                    action();
                }
                catch (CommandFailedException ex)
                {
                    logMethod(label + ": " + ex);
                }

                // NOTE: Don't lock here or we'll have deadlock with Cleanup() which is running on a diff thread.
                // As long as we set them in the same order they're used in Cleanup(), we can safely avoid
                // this lock.
                //lock (currentOperationFinished)
                {
                    logMethod("End cleared currentOperationStarted: " + label);
                    currentOperationStarted = false;
                    currentOperationFinished.Set();
                }
            }

            public void Stop(VoidDelegate waitForChildren, VoidDelegate teardown)
            {
                logMethod(string.Format("STOP: state={0}", state));
                switch (state)
                {
                    case State.Started:
                        Cleanup(waitForChildren);
                        break;
                    case State.Stopping:
                        waitForStopped.WaitOne();
                        break;
                    case State.Stopped:
                        break;
                }
                // State is now Stopped

                if(teardown!=null)
                {
                    logMethod(string.Format("TEARDOWN: state={0}", state));
                    teardown();
                }
            }

            WaitHandle IStoppable.WaitForStopped { get { return waitForStopped; } }

            public bool IsRunning
            {
                get
                {
                    lock (stateChangeLock)
                    {
                        return state == State.Started;
                    }
                }
            }

            private void Cleanup(VoidDelegate waitForChildren)
            {
                lock (stateChangeLock)
                {
                    logMethod(string.Format("Cleanup: state={0}", state));
                    state = State.Stopping;

                    // Wait for current operation to stop, if there is one.
                    lock (currentOperationFinished)
                    {
                        if (currentOperationStarted)
                        {
                            logMethod("Cleanup thinks there's an operation running...");
                            currentOperationFinished.WaitOne(30 * 1000, false);
                        }
                    }

                    if (waitForChildren != null)
                    {
                        logMethod("Waiting for children to stop");
                        waitForChildren();
                    }

                    // Signal that we are done
                    state = State.Stopped;
                    waitForStopped.Set();
                }
            }
        }

        internal delegate void VoidDelegate();

        internal interface IStoppable
        {
            WaitHandle WaitForStopped { get; }
            void Stop(VoidDelegate waitForChildren, VoidDelegate teardown);
        }
    }
}
