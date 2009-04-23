﻿//-----------------------------------------------------------------------
// <copyright file="SimplePerfTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
	/// <summary>
	/// Basic performance tests
	/// </summary>
	[TestClass]
	public class SimplePerfTest
	{
		const int DataSize = 32;

		private Instance instance;
		private Session session;
		private Table table;

		private JET_COLUMNID columnidKey;
		private JET_COLUMNID columnidData;

		// Used to insert records
		private long nextKey = 0;
		private byte[] data;

		// Used to retrieve records
		private byte[] keyBuf;
		private byte[] dataBuf;

		private Random random;

		/// <summary>
		/// Setup for a test -- this creates the database
		/// </summary>
		[TestInitialize]
		public void Setup()
		{
			this.random = new Random();
			this.data = new byte[DataSize];
			this.random.NextBytes(this.data);

			this.keyBuf = new byte[8];
			this.dataBuf = new byte[DataSize];

			JET_DBID dbid;

			this.instance = new Instance("SimplePerfTest");

			// Circular logging, 16MB logfiles, 8MB of log buffer
			this.instance.Parameters.CircularLog = true;
			this.instance.Parameters.LogFileSize = 16 * 1024; // in KB
			this.instance.Parameters.LogBuffers = 16 * 1024; // in 512-byte units

			// Create the instance, database and table
			this.instance.Init();
			this.session = new Session(this.instance);
			Api.JetCreateDatabase(this.session, "esentperftest.db", string.Empty, out dbid, CreateDatabaseGrbit.None);

			// Create the table
			using (var trx = new Transaction(this.session))
			{
				JET_TABLEID tableid;
				var columndef = new JET_COLUMNDEF();

				Api.JetCreateTable(this.session, dbid, "table", 0, 100, out tableid);
				columndef.coltyp = JET_coltyp.Currency;
				Api.JetAddColumn(this.session, tableid, "Key", columndef, null, 0, out this.columnidKey);
				columndef.coltyp = JET_coltyp.Binary;
				Api.JetAddColumn(this.session, tableid, "Data", columndef, null, 0, out this.columnidData);
				Api.JetCreateIndex(this.session, tableid, "primary", CreateIndexGrbit.IndexPrimary, "+key\0\0", 6, 100);
				Api.JetCloseTable(this.session, tableid);
				trx.Commit(CommitTransactionGrbit.None);
			}

			this.table = new Table(this.session, dbid, "table", OpenTableGrbit.None);
		}

		/// <summary>
		/// Cleanup after the test
		/// </summary>
		[TestCleanup]
		public void Teardown()
		{
			this.table.Close();
			this.session.End();
			this.instance.Term();
		}

		/// <summary>
		/// Test inserting and retrieving records.
		/// </summary>
		[TestMethod]
		[Priority(2)]
		public void BasicPerfTest()
		{
			this.RunGarbageCollection();
			long memoryAtStart = GC.GetTotalMemory(true);

			const int numRecords = 1000000;

			TimeAction("Insert records", () => this.InsertRecords(numRecords));
			TimeAction("Read one record", () => this.RepeatedlyRetrieveOneRecord(numRecords));
			TimeAction("Read all records", this.RetrieveAllRecords);

			// Randomly seek to all records in the table
			long[] keys = (from x in Enumerable.Range(0, numRecords) select (long)x).ToArray();
			this.Suffle(keys);

			TimeAction("Seek to all records", () => this.SeekToAllRecords(keys));

			this.RunGarbageCollection();
			long memoryAtEnd = GC.GetTotalMemory(true);
			Console.WriteLine("Memory changed by {0} bytes", memoryAtEnd - memoryAtStart);
		}

		private void Suffle<T>(T[] arrayToShuffle)
		{
			for(int i = 0; i < arrayToShuffle.Length; ++i)
			{
				int swap = this.random.Next(i, arrayToShuffle.Length);
				T temp = arrayToShuffle[i];
				arrayToShuffle[i] = arrayToShuffle[swap];
				arrayToShuffle[swap] = temp;
			}
		}

		private static void TimeAction(string name, Action action)
		{
			var stopwatch = Stopwatch.StartNew();
			action();
			stopwatch.Stop();
			Console.WriteLine("{0}: {1}", name, stopwatch.Elapsed);
		}

		private void InsertRecord()
		{			
			long key = this.nextKey++;		
			Api.JetPrepareUpdate(this.session, this.table, JET_prep.Insert);
			Api.SetColumn(this.session, this.table, this.columnidKey, key);
			Api.SetColumn(this.session, this.table, this.columnidData, this.data);
			Api.JetUpdate(this.session, this.table);
		}

		private void InsertRecords(int numRecords)
		{
			for (int i = 0; i < numRecords; ++i)
			{
				using (Transaction trx = new Transaction(this.session))
				{
					this.InsertRecord();
					trx.Commit(CommitTransactionGrbit.LazyFlush);
				}
			}
		}

		private void RetrieveRecord()
		{
			int actualSize;
			Api.RetrieveColumnAsInt64(this.session, this.table, this.columnidKey);
			Api.JetRetrieveColumn(
				this.session,
				this.table,
				this.columnidData,
				this.dataBuf,
				this.dataBuf.Length,
				out actualSize,
				RetrieveColumnGrbit.None,
				null);
		}

		private void RetrieveAllRecords()
		{
			Api.MoveBeforeFirst(this.session, this.table);
			while (Api.TryMoveNext(this.session, this.table))
			{
				using (var trx = new Transaction(this.session))
				{
					this.RetrieveRecord();
					trx.Commit(CommitTransactionGrbit.None);
				}
			}
		}

		private void RepeatedlyRetrieveOneRecord(int numRetrieves)
		{
			Api.JetMove(this.session, this.table, JET_Move.First, MoveGrbit.None);
			for (int i = 0; i < numRetrieves; ++i)
			{
				using (var trx = new Transaction(this.session))
				{
					this.RetrieveRecord();
					trx.Commit(CommitTransactionGrbit.None);
				}
			}
		}

		private void SeekToAllRecords(long[] keys)
		{					
			foreach(long key in keys)
			{
				using (var trx = new Transaction(this.session))
				{
					Api.MakeKey(this.session, this.table, key, MakeKeyGrbit.NewKey);
					Api.JetSeek(this.session, this.table, SeekGrbit.SeekEQ);
					Assert.AreEqual(key, Api.RetrieveColumnAsInt64(this.session, this.table, this.columnidKey));
					trx.Commit(CommitTransactionGrbit.None);
				}
			}	
		}

		private void RunGarbageCollection()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
