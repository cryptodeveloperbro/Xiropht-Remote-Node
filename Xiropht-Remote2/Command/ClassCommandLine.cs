﻿using System;
using System.Threading;
using Xiropht_Connector_All.Utils;
using Xiropht_Remote2.Api;
using Xiropht_Remote2.Data;
using Xiropht_Remote2.RemoteNode;

namespace Xiropht_Remote2.Command
{
    public class ClassCommandLine
    {
        public static bool CommandLine(string command)
        {
            var splitCommand = command.Split(new char[0], StringSplitOptions.None);
            try
            {
                switch (splitCommand[0])
                {
                    case "help":
                        Console.WriteLine("Command list: ");
                        Console.WriteLine("status -> Get Network status of your node");
                        Console.WriteLine("transaction -> Get the number of transaction(s) sync.");
                        Console.WriteLine("block -> Get the number of block(s) sync.");
                        Console.WriteLine("log -> Can set level of log to show: (default) log 0 max level 4");
                        Console.WriteLine("clearsync -> Clear the sync of the remote node.");
                        Console.WriteLine("save -> Save sync.");
                        Console.WriteLine("exit -> Save sync and Exit the node.");
                        break;
                    case "status":
                        Console.WriteLine("Total Transaction Sync: " + (ClassRemoteNodeSync.ListOfTransaction.Count));
                        Console.WriteLine("Total Transaction in the Blockchain: " + ClassRemoteNodeSync.TotalTransaction);
                        int totalTransactionSortedPerWallet = 0;
                        foreach (var transactionSortedEntry in ClassRemoteNodeSync.ListTransactionPerWallet)
                        {
                            totalTransactionSortedPerWallet += transactionSortedEntry.Value.Count;
                        }
                        Console.WriteLine("Total Transaction Sorted for Wallet(s): " + totalTransactionSortedPerWallet);
                        Console.WriteLine("Total Block(s) Sync: " + (ClassRemoteNodeSync.ListOfBlock.Count));
                        Console.WriteLine("Total Block(s) mined in the Blockchain: " + ClassRemoteNodeSync.TotalBlockMined);
                        Console.WriteLine("Total Block(s) left to mining: " + ClassRemoteNodeSync.CurrentBlockLeft);
                        Console.WriteLine("Total pending transaction in the network: " + ClassRemoteNodeSync.TotalPendingTransaction);
                        Console.WriteLine("Total Fee in the network: " + ClassRemoteNodeSync.CurrentTotalFee);
                        Console.WriteLine("Current Mining Difficulty: " + ClassRemoteNodeSync.CurrentDifficulty);
                        if (ClassRemoteNodeSync.CurrentHashrate != null)
                        {
                            Console.WriteLine("Current Mining Hashrate: " + ClassUtils.GetTranslateHashrate(ClassRemoteNodeSync.CurrentHashrate.Replace(".", ","), 2).Replace(",", "."));
                        }
                        Console.WriteLine("Total Coin Max Supply: " + ClassUtils.GetTranslateBigNumber(ClassRemoteNodeSync.CoinMaxSupply.Replace(".", ",")).Replace(",", "."));
                        Console.WriteLine("Total Coin Circulating: " + ClassRemoteNodeSync.CoinCirculating);

                        if (ClassRemoteNodeSync.WantToBePublicNode)
                        {
                            string publicNodes = string.Empty;
                            for (int i = 0; i < ClassRemoteNodeSync.ListOfPublicNodes.Count; i++)
                            {
                                if (i < ClassRemoteNodeSync.ListOfPublicNodes.Count)
                                {
                                    publicNodes += ClassRemoteNodeSync.ListOfPublicNodes[i] + " ";
                                }
                            }
                            Console.WriteLine("List of Public Remote Node: " + publicNodes);
                            string status = "NOT LISTED";
                            if (ClassRemoteNodeSync.ImPublicNode)
                            {
                                status = "LISTED";
                            }
                            Console.WriteLine("Public Status of the Remote Node: " + status);

                        }
                        Console.WriteLine("Trusted Key: " + ClassRemoteNodeSync.TrustedKey);
                        Console.WriteLine("Hash Transaction Key: " + ClassRemoteNodeSync.HashTransactionList);
                        Console.WriteLine("Hash Block Key: " + ClassRemoteNodeSync.HashBlockList);
                        break;
                    case "transaction":
                        Console.WriteLine("Total Transaction Sync: " + (ClassRemoteNodeSync.ListOfTransaction.Count));
                        Console.WriteLine("Total Transaction in the Blockchain: " + ClassRemoteNodeSync.TotalTransaction);
                        break;
                    case "block":
                        Console.WriteLine("Total Block(s) Sync: " + (ClassRemoteNodeSync.ListOfBlock.Count));
                        Console.WriteLine("Total Block(s) mined in the Blockchain: " + ClassRemoteNodeSync.TotalBlockMined);
                        Console.WriteLine("Total Block(s) left to mining: " + ClassRemoteNodeSync.CurrentBlockLeft);
                        break;
                    case "log":
                        if (!string.IsNullOrEmpty(splitCommand[1]))
                        {
                            if (int.TryParse(splitCommand[1], out var logLevel))
                            {
                                if (logLevel < 0)
                                {
                                    logLevel = 0;
                                }
                                if (logLevel > 5)
                                {
                                    logLevel = 5;
                                }
                                Console.WriteLine("Log Level " + Program.LogLevel + " -> " + logLevel);
                                Program.LogLevel = logLevel;
                            }
                            else
                            {
                                Console.WriteLine("Wrong argument: " + splitCommand[1] + " should be a number.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Empty/Missing argument.");
                        }
                        break;
                    case "clearsync":
                        ClassRemoteNodeSync.ListOfBlock.Clear();
                        ClassRemoteNodeSync.ListOfTransaction.Clear();
                        ClassRemoteNodeSync.ListTransactionPerWallet.Clear();
                        ClassRemoteNodeKey.DataBlockRead = string.Empty;
                        ClassRemoteNodeKey.DataTransactionRead = string.Empty;
                        ClassRemoteNodeSave.TotalBlockSaved = 0;
                        ClassRemoteNodeSave.TotalTransactionSaved = 0;
                        ClassRemoteNodeSave.DataTransactionSaved = string.Empty;
                        ClassRemoteNodeSave.DataBlockSaved = string.Empty;
                        ClassRemoteNodeKey.LastBlockIdRead = 0;
                        ClassRemoteNodeKey.LastTransactionIdRead = 0;

                        Console.WriteLine("Clear finish, restart sync..");
                        ClassRemoteNodeKey.StartUpdateHashTransactionList();
                        ClassRemoteNodeKey.StartUpdateTrustedKey();
                        break;
                    case "save":
                        Console.WriteLine("Starting save sync..");
                        while (ClassRemoteNodeSave.InSaveTransactionDatabase)
                        {
                            Thread.Sleep(1000);
                        }
                        ClassRemoteNodeSave.TotalTransactionSaved = 0;
                        ClassRemoteNodeSave.DataTransactionSaved = string.Empty;
                        ClassRemoteNodeSave.SaveTransaction(false);
                        while (ClassRemoteNodeSave.InSaveBlockDatabase)
                        {
                            Thread.Sleep(1000);
                        }
                        ClassRemoteNodeSave.TotalBlockSaved = 0;
                        ClassRemoteNodeSave.DataBlockSaved = string.Empty;
                        ClassRemoteNodeSave.SaveBlock(false);
                        Console.WriteLine("Sync saved.");
                        break;
                    case "exit":
                        Program.Closed = true;
                        Console.WriteLine("Disable auto reconnect remote node..");
                        ClassCheckRemoteNodeSync.DisableCheckRemoteNodeSync();
                        Thread.Sleep(1000);
                        Console.WriteLine("Stop each connection of the remote node.");
                        Program.RemoteNodeObjectBlock.StopConnection();
                        Program.RemoteNodeObjectCoinCirculating.StopConnection();
                        Program.RemoteNodeObjectCoinMaxSupply.StopConnection();
                        Program.RemoteNodeObjectCurrentDifficulty.StopConnection();
                        Program.RemoteNodeObjectCurrentRate.StopConnection();
                        Program.RemoteNodeObjectToBePublic.StopConnection();
                        Program.RemoteNodeObjectTotalBlockMined.StopConnection();
                        Program.RemoteNodeObjectTotalFee.StopConnection();
                        Program.RemoteNodeObjectTotalPendingTransaction.StopConnection();
                        Program.RemoteNodeObjectTotalTransaction.StopConnection();
                        Program.RemoteNodeObjectTransaction.StopConnection();
                        Thread.Sleep(1000);
                        Console.WriteLine("Stop api..");
                        ClassApi.StopApi();
                        ClassRemoteNodeSave.TotalBlockSaved = 0;
                        ClassRemoteNodeSave.DataBlockSaved = string.Empty;
                        Console.WriteLine("Waiting end of save block database..");
                        while(ClassRemoteNodeSave.InSaveBlockDatabase)
                        {
                            Thread.Sleep(100);
                        }
                        if (ClassRemoteNodeSave.SaveBlock(false))
                        {
                            Console.WriteLine("save block database successfully done.");
                        }
                        else
                        {
                            Console.WriteLine("save block database error.");
                        }
                        ClassRemoteNodeSave.TotalTransactionSaved = 0;
                        ClassRemoteNodeSave.DataTransactionSaved = string.Empty;
                        Console.WriteLine("Waiting end of save transaction database..");
                        while (ClassRemoteNodeSave.InSaveTransactionDatabase)
                        {
                            Thread.Sleep(100);
                        }
                        if (ClassRemoteNodeSave.SaveTransaction(false))
                        {
                            Console.WriteLine("save transaction database successfully done.");
                        }
                        else
                        {
                            Console.WriteLine("save transaction database error.");
                        }
                        Console.WriteLine("Sync saved and exit.");
                        Environment.Exit(0);
                        return false;
                }
            }
            catch(Exception error)
            {
                Console.WriteLine("Command line error: "+error.Message);
            }
            return true;
        }
    }
}