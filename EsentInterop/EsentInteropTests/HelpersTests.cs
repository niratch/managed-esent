﻿//-----------------------------------------------------------------------
// <copyright file="HelpersTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Tests for the various Set/RetrieveColumn* methods and
    /// the helper methods that retrieve meta-data.
    /// </summary>
    [TestClass]
    public class HelpersTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The name of the table.
        /// </summary>
        private string table;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesid;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// The tableid being used by the test.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// A dictionary that maps column names to column ids.
        /// </summary>
        private IDictionary<string, JET_COLUMNID> columnidDict;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.PageTempDBMin, SystemParameters.PageTempDBSmallest, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            JET_COLUMNDEF columndef = null;
            JET_COLUMNID columnid;
            
            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Bit };
            Api.JetAddColumn(this.sesid, this.tableid, "Boolean", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.UnsignedByte };
            Api.JetAddColumn(this.sesid, this.tableid, "Byte", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Short };
            Api.JetAddColumn(this.sesid, this.tableid, "Int16", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Long };
            Api.JetAddColumn(this.sesid, this.tableid, "Int32", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Currency };
            Api.JetAddColumn(this.sesid, this.tableid, "Int64", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEESingle };
            Api.JetAddColumn(this.sesid, this.tableid, "Float", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEEDouble };
            Api.JetAddColumn(this.sesid, this.tableid, "Double", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.DateTime };
            Api.JetAddColumn(this.sesid, this.tableid, "DateTime", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongBinary };
            Api.JetAddColumn(this.sesid, this.tableid, "Binary", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.ASCII };
            Api.JetAddColumn(this.sesid, this.tableid, "ASCII", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode };
            Api.JetAddColumn(this.sesid, this.tableid, "Unicode", columndef, null, 0, out columnid);

            if (EsentVersion.SupportsVistaFeatures)
            {
                // Starting with windows Vista esent provides support for these columns.) 
                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.UnsignedShort };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.UnsignedLong };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.GUID };
                Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);
            }
            else
            {
                // Older version of esent don't support these column types natively so we'll just use binary columns.
                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 2 };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 4 };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 16 };
                Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);
            }

            // Not natively supported by any version of Esent
            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 8 };
            Api.JetAddColumn(this.sesid, this.tableid, "UInt64", columndef, null, 0, out columnid);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);

            this.columnidDict = Api.GetColumnDictionary(this.sesid, this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyFixtureSetup()
        {
            Assert.IsNotNull(this.table);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_DBID.Nil, this.dbid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
            Assert.IsNotNull(this.columnidDict);

            Assert.IsTrue(this.columnidDict.ContainsKey("boolean"));
            Assert.IsTrue(this.columnidDict.ContainsKey("byte"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int64"));
            Assert.IsTrue(this.columnidDict.ContainsKey("float"));
            Assert.IsTrue(this.columnidDict.ContainsKey("double"));
            Assert.IsTrue(this.columnidDict.ContainsKey("binary"));
            Assert.IsTrue(this.columnidDict.ContainsKey("ascii"));
            Assert.IsTrue(this.columnidDict.ContainsKey("unicode"));
            Assert.IsTrue(this.columnidDict.ContainsKey("guid"));
            Assert.IsTrue(this.columnidDict.ContainsKey("datetime"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint64"));

            Assert.IsFalse(this.columnidDict.ContainsKey("nosuchcolumn"));
        }

        #endregion Setup/Teardown

        #region RetrieveColumn tests

        /// <summary>
        /// Check that retrieving the size of a null column returns 0
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullColumnSize()
        {
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            Assert.AreEqual(0, Api.RetrieveColumnSize(this.sesid, this.tableid, this.columnidDict["Int32"]));
        }

        /// <summary>
        /// Check that retrieving the size of a column returns the amount of data
        /// in the column.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveColumnSize()
        {
            JET_COLUMNID columnid = this.columnidDict["Byte"];
            var b = new byte[] { 0x55 };
            this.InsertRecord(columnid, b);
            Assert.AreEqual(0, Api.RetrieveColumnSize(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column that exceeds the cached buffer size used by RetrieveColumn
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveLargeColumn()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var expected = new byte[16384];

            var random = new Random();
            random.NextBytes(expected);

            this.InsertRecord(columnid, expected);

            byte[] actual = Api.RetrieveColumn(this.sesid, this.tableid, columnid);
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Check that retrieving a column returns null
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullColumn()
        {
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            Assert.IsNull(Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidDict["Int32"]));
        }

        /// <summary>
        /// Retrieve a column as boolean.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsBoolean()
        {
            JET_COLUMNID columnid = this.columnidDict["Boolean"];
            bool value = Any.Boolean;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsBoolean(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as boolean.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsBoolean()
        {
            this.NullColumnTest<bool>("Boolean", Api.RetrieveColumnAsBoolean);
        }

        /// <summary>
        /// Retrieve a column as a byte.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsByte()
        {
            JET_COLUMNID columnid = this.columnidDict["Byte"];
            var b = new byte[] { 0x55 };
            this.InsertRecord(columnid, b);
            Assert.AreEqual(b[0], Api.RetrieveColumnAsByte(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as byte.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsByte()
        {
            this.NullColumnTest<byte>("Byte", Api.RetrieveColumnAsByte);
        }

        /// <summary>
        /// Retrieve a column as a short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsInt16()
        {
            JET_COLUMNID columnid = this.columnidDict["Int16"];
            short value = Any.Int16;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsInt16(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsInt16()
        {
            this.NullColumnTest<short>("Int16", Api.RetrieveColumnAsInt16);
        }

        /// <summary>
        /// Retrieving a byte as a short throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsInt16ThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsInt16(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsUInt16()
        {
            JET_COLUMNID columnid = this.columnidDict["UInt16"];
            ushort value = Any.UInt16;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt16(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsUInt16()
        {
            this.NullColumnTest<ushort>("UInt16", Api.RetrieveColumnAsUInt16);
        }

        /// <summary>
        /// Retrieving a byte as a ushort throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsUInt16ThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsUInt16(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsInt32()
        {
            JET_COLUMNID columnid = this.columnidDict["Int32"];
            int value = Any.Int32;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsInt32()
        {
            this.NullColumnTest<int>("Int32", Api.RetrieveColumnAsInt32);
        }

        /// <summary>
        /// Retrieving a byte as an int throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsInt32ThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsInt32(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a uint.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsUInt32()
        {
            JET_COLUMNID columnid = this.columnidDict["UInt32"];
            uint value = Any.UInt32;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt32(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a uint.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsUInt32()
        {
            this.NullColumnTest<uint>("UInt32", Api.RetrieveColumnAsUInt32);
        }

        /// <summary>
        /// Retrieving a byte as a uint throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsUInt32ThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsUInt32(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a long.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsInt64()
        {
            JET_COLUMNID columnid = this.columnidDict["Int64"];
            long value = Any.Int64;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a long.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsInt64()
        {
            this.NullColumnTest<long>("Int64", Api.RetrieveColumnAsInt64);
        }

        /// <summary>
        /// Retrieving a byte as a long throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsInt64ThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsInt64(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a ulong.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsUInt64()
        {
            JET_COLUMNID columnid = this.columnidDict["UInt64"];
            ulong value = Any.UInt64;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt64(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a ulong.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsUInt64()
        {
            this.NullColumnTest<ulong>("UInt64", Api.RetrieveColumnAsUInt64);
        }

        /// <summary>
        /// Retrieving a byte as a ulong throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsUInt64ThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsUInt64(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a float.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsFloat()
        {
            JET_COLUMNID columnid = this.columnidDict["Float"];
            float value = Any.Float;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsFloat(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a float.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsFloat()
        {
            this.NullColumnTest<float>("float", Api.RetrieveColumnAsFloat);
        }

        /// <summary>
        /// Retrieving a byte as a float throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsFloatThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsFloat(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a double.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsDouble()
        {
            JET_COLUMNID columnid = this.columnidDict["Double"];
            double value = Any.Double;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsDouble(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a double.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsDouble()
        {
            this.NullColumnTest<double>("double", Api.RetrieveColumnAsDouble);
        }

        /// <summary>
        /// Retrieving a byte as a double throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsDoubleThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsDouble(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a Guid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsGuid()
        {
            JET_COLUMNID columnid = this.columnidDict["Guid"];
            Guid value = Any.Guid;
            this.InsertRecord(columnid, value.ToByteArray());
            Assert.AreEqual(value, Api.RetrieveColumnAsGuid(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a guid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsGuid()
        {
            this.NullColumnTest<Guid>("Guid", Api.RetrieveColumnAsGuid);
        }

        /// <summary>
        /// Retrieving a byte as a guid throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsGuidThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsGuid(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as a DateTime.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsDateTime()
        {
            JET_COLUMNID columnid = this.columnidDict["DateTime"];

            // The .NET DateTime class has more precision than ESENT can store so we can't use
            // a general time (e.g. DateTime.Now) here
            var value = new DateTime(2006, 09, 10, 4, 5, 6);
            this.InsertRecord(columnid, BitConverter.GetBytes(value.ToOADate()));
            Assert.AreEqual(value, Api.RetrieveColumnAsDateTime(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a DateTime when the value is invalid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsDateTimeReturnsMinWhenValueIsTooSmall()
        {
            JET_COLUMNID columnid = this.columnidDict["DateTime"];

            // MSDN says that the value must be a value between negative 657435.0 through positive 2958466.0
            this.InsertRecord(columnid, BitConverter.GetBytes(-657436.0));
            Assert.AreEqual(DateTime.MinValue, Api.RetrieveColumnAsDateTime(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a DateTime when the value is invalid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsDateTimeReturnsMaxWhenValueIsTooLarge()
        {
            JET_COLUMNID columnid = this.columnidDict["DateTime"];

            // MSDN says that the value must be a value between negative 657435.0 through positive 2958466.0
            this.InsertRecord(columnid, BitConverter.GetBytes(2958467.0));
            Assert.AreEqual(DateTime.MaxValue, Api.RetrieveColumnAsDateTime(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a null column as a DateTime.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsDateTime()
        {
            this.NullColumnTest<DateTime>("DateTime", Api.RetrieveColumnAsDateTime);
        }

        /// <summary>
        /// Retrieving a byte as a DateTime throws an exception when the column
        /// is too short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentInvalidColumnException))]
        public void VerifyRetrieveAsDateTimeThrowsExceptionWhenColumnIsTooShort()
        {
            JET_COLUMNID columnid = this.columnidDict["Binary"];
            var value = new byte[1];
            this.InsertRecord(columnid, value);
            Api.RetrieveColumnAsDateTime(this.sesid, this.tableid, columnid);
        }

        /// <summary>
        /// Retrieve a column as ASCII
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsAscii()
        {
            JET_COLUMNID columnid = this.columnidDict["ASCII"];
            string value = Any.String;
            this.InsertRecord(columnid, Encoding.ASCII.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.ASCII));
        }

        /// <summary>
        /// Retrieve a null column as ASCII
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsAscii()
        {
            JET_COLUMNID columnid = this.columnidDict["ASCII"];
            this.InsertRecord(columnid, null);
            Assert.IsNull(Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.ASCII));
        }

        /// <summary>
        /// Retrieve a column as Unicode
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsUnicode()
        {
            JET_COLUMNID columnid = this.columnidDict["Unicode"];
            string value = Any.String;
            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.Unicode));
        }

        /// <summary>
        /// Retrieve a null column as Unicode
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveNullAsUnicode()
        {
            JET_COLUMNID columnid = this.columnidDict["Unicode"];
            this.InsertRecord(columnid, null);
            Assert.IsNull(Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.Unicode));
        }

        /// <summary>
        /// Retrieve a column as a (unicode) string
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveAsString()
        {
            JET_COLUMNID columnid = this.columnidDict["Unicode"];
            string value = Any.String;
            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a string that is too large for the cached retrieval buffer
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveLargeString()
        {
            JET_COLUMNID columnid = this.columnidDict["Unicode"];
            var value = Any.StringOfLength(16384);
            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a string that is too large for the cached retrieval buffer
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveExtremelyLargeString()
        {
            JET_COLUMNID columnid = this.columnidDict["Unicode"];
            var value = Any.StringOfLength(1024 * 1024);
            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve an empty string to make sure
        /// it is handled differently from a null column.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void RetrieveEmptyString()
        {
            JET_COLUMNID columnid = this.columnidDict["Unicode"];
            string value = String.Empty;
            byte[] data = Encoding.Unicode.GetBytes(value);
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetSetColumn(this.sesid, this.tableid, columnid, data, data.Length, SetColumnGrbit.ZeroLength, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.Unicode));
        }

        #endregion RetrieveColumnAs tests

        #region SetColumn Tests

        /// <summary>
        /// Test setting a unicode column from a string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetUnicodeString()
        {
            JET_COLUMNID columnid = this.columnidDict["unicode"];
            string expected = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected, Encoding.Unicode);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            
            string actual = Encoding.Unicode.GetString(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a string
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithString()
        {
            JET_COLUMNID columnid = this.columnidDict["unicode"];
            var value = Any.String;
            this.InsertRecordWithSetColumns(columnid, new StringColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting an ASCII column from a string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetASCIIString()
        {
            JET_COLUMNID columnid = this.columnidDict["ascii"];
            string expected = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected, Encoding.ASCII);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            string actual = Encoding.ASCII.GetString(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Using an encoding which is neither ASCII nor Unicode should thrown an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifySetStringWithInvalidEncodingThrowsException()
        {
            JET_COLUMNID columnid = this.columnidDict["unicode"];

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);

            try
            {
                Api.SetColumn(this.sesid, this.tableid, columnid, Any.String, Encoding.UTF8);
                Assert.Fail("Expected an ESENT exception");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /// <summary>
        /// Test setting a column from an empty string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetEmptyString()
        {
            JET_COLUMNID columnid = this.columnidDict["unicode"];
            string expected = string.Empty;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected, Encoding.Unicode);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            string actual = Encoding.Unicode.GetString(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a null string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetNullString()
        {
            JET_COLUMNID columnid = this.columnidDict["unicode"];

            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(Any.String));

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
            Api.SetColumn(this.sesid, this.tableid, columnid, null, Encoding.Unicode);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.IsNull(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a boolean.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetBooleanTrue()
        {
            JET_COLUMNID columnid = this.columnidDict["boolean"];
            bool expected = true;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            bool actual = BitConverter.ToBoolean(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a boolean.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetBooleanFalse()
        {
            JET_COLUMNID columnid = this.columnidDict["boolean"];
            bool expected = false;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            bool actual = BitConverter.ToBoolean(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a boolean
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithBoolean()
        {
            JET_COLUMNID columnid = this.columnidDict["Boolean"];
            bool value = Any.Boolean;
            this.InsertRecordWithSetColumns(columnid, new BoolColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsBoolean(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test SetColumn with a byte
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetByte()
        {
            JET_COLUMNID columnid = this.columnidDict["byte"];
            byte expected = Any.Byte;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            byte actual = Api.RetrieveColumn(this.sesid, this.tableid, columnid)[0];
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a byte
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithByte()
        {
            JET_COLUMNID columnid = this.columnidDict["byte"];
            var value = Any.Byte;
            this.InsertRecordWithSetColumns(columnid, new ByteColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsByte(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetInt16()
        {
            JET_COLUMNID columnid = this.columnidDict["int16"];
            short expected = Any.Int16;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            short actual = BitConverter.ToInt16(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with an Int16
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithInt16()
        {
            JET_COLUMNID columnid = this.columnidDict["int16"];
            var value = Any.Int16;
            this.InsertRecordWithSetColumns(columnid, new Int16ColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsInt16(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetInt32()
        {
            JET_COLUMNID columnid = this.columnidDict["int32"];
            int expected = Any.Int32;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            int actual = BitConverter.ToInt32(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with an Int32
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithInt32()
        {
            JET_COLUMNID columnid = this.columnidDict["int32"];
            var value = Any.Int32;
            this.InsertRecordWithSetColumns(columnid, new Int32ColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a long.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetInt64()
        {
            JET_COLUMNID columnid = this.columnidDict["int64"];
            long expected = Any.Int64;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            long actual = BitConverter.ToInt64(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with an Int64
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithInt64()
        {
            JET_COLUMNID columnid = this.columnidDict["int64"];
            var value = Any.Int64;
            this.InsertRecordWithSetColumns(columnid, new Int64ColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetUInt16()
        {
            JET_COLUMNID columnid = this.columnidDict["uint16"];
            ushort expected = Any.UInt16;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            ushort actual = BitConverter.ToUInt16(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a UInt16
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithUInt16()
        {
            JET_COLUMNID columnid = this.columnidDict["uint16"];
            var value = Any.UInt16;
            this.InsertRecordWithSetColumns(columnid, new UInt16ColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt16(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a uint.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetUInt32()
        {
            JET_COLUMNID columnid = this.columnidDict["uint32"];
            uint expected = Any.UInt32;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            uint actual = BitConverter.ToUInt32(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a UInt32
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithUInt32()
        {
            JET_COLUMNID columnid = this.columnidDict["uint32"];
            var value = Any.UInt32;
            this.InsertRecordWithSetColumns(columnid, new UInt32ColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt32(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a ulong.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetUInt64()
        {
            JET_COLUMNID columnid = this.columnidDict["uint64"];
            ulong expected = Any.UInt64;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            ulong actual = BitConverter.ToUInt64(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a UInt64
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithUInt64()
        {
            JET_COLUMNID columnid = this.columnidDict["uint64"];
            var value = Any.UInt64;
            this.InsertRecordWithSetColumns(columnid, new UInt64ColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt64(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a float.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetFloat()
        {
            JET_COLUMNID columnid = this.columnidDict["float"];
            float expected = Any.Float;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            float actual = BitConverter.ToSingle(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a Float
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithFloat()
        {
            JET_COLUMNID columnid = this.columnidDict["Float"];
            var value = Any.Float;
            this.InsertRecordWithSetColumns(columnid, new FloatColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsFloat(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a double.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetDouble()
        {
            JET_COLUMNID columnid = this.columnidDict["double"];
            double expected = Any.Double;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            double actual = BitConverter.ToDouble(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a Double
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithDouble()
        {
            JET_COLUMNID columnid = this.columnidDict["Double"];
            var value = Any.Double;
            this.InsertRecordWithSetColumns(columnid, new DoubleColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsDouble(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a guid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetGuid()
        {
            JET_COLUMNID columnid = this.columnidDict["guid"];
            Guid expected = Any.Guid;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = new Guid(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a Guid
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithGuid()
        {
            JET_COLUMNID columnid = this.columnidDict["Guid"];
            var value = Any.Guid;
            this.InsertRecordWithSetColumns(columnid, new GuidColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsGuid(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a DateTime.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetDateTime()
        {
            JET_COLUMNID columnid = this.columnidDict["DateTime"];
            var expected = new DateTime(1956, 01, 02, 13, 2, 59);

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            DateTime actual = DateTime.FromOADate(BitConverter.ToDouble(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a DateTime
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithDateTime()
        {
            JET_COLUMNID columnid = this.columnidDict["DateTime"];
            var value = Any.DateTime;
            this.InsertRecordWithSetColumns(columnid, new DateTimeColumnValue { Columnid = columnid, Value = value });
            Assert.AreEqual(value, Api.RetrieveColumnAsDateTime(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from an array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetBytes()
        {
            JET_COLUMNID columnid = this.columnidDict["binary"];
            byte[] expected = Any.Bytes;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            byte[] actual = Api.RetrieveColumn(this.sesid, this.tableid, columnid);
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a ColumnValue with a byte array
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnsWithBytes()
        {
            JET_COLUMNID columnid = this.columnidDict["binary"];
            var value = Any.Bytes;
            this.InsertRecordWithSetColumns(columnid, new BytesColumnValue { Columnid = columnid, Value = value });
            CollectionAssert.AreEqual(value, Api.RetrieveColumn(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a binary column from a zero-length array.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetZeroLengthBytes()
        {
            JET_COLUMNID columnid = this.columnidDict["binary"];

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, new byte[0]);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.AreEqual(0, Api.RetrieveColumn(this.sesid, this.tableid, columnid).Length);
        }

        /// <summary>
        /// Test setting a binary column from a null object.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetNullBytes()
        {
            JET_COLUMNID columnid = this.columnidDict["binary"];

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.IsNull(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
        }

        #endregion SetColumn Tests

        #region MakeKey Tests

        /// <summary>
        /// Test make a key from true.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyBooleanTrue()
        {
            this.CreateIndexOnColumn("boolean");
            Api.MakeKey(this.sesid, this.tableid, true, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a boolean.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyBooleanFalse()
        {
            this.CreateIndexOnColumn("boolean");
            Api.MakeKey(this.sesid, this.tableid, false, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a byte.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyByte()
        {
            this.CreateIndexOnColumn("byte");
            Api.MakeKey(this.sesid, this.tableid, Any.Byte, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyInt16()
        {
            this.CreateIndexOnColumn("int16");
            Api.MakeKey(this.sesid, this.tableid, Any.Int16, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyUInt16()
        {
            this.CreateIndexOnColumn("uint16");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt16, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyInt32()
        {
            this.CreateIndexOnColumn("int32");
            Api.MakeKey(this.sesid, this.tableid, Any.Int32, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a uint.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyUInt32()
        {
            this.CreateIndexOnColumn("uint32");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt32, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a long.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyInt64()
        {
            this.CreateIndexOnColumn("int64");
            Api.MakeKey(this.sesid, this.tableid, Any.Int64, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a ulong.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyUInt64()
        {
            this.CreateIndexOnColumn("uint64");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt64, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a float.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyFloat()
        {
            this.CreateIndexOnColumn("float");
            Api.MakeKey(this.sesid, this.tableid, Any.Float, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a double.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyDouble()
        {
            this.CreateIndexOnColumn("double");
            Api.MakeKey(this.sesid, this.tableid, Any.Double, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a guid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyGuid()
        {
            this.CreateIndexOnColumn("guid");
            Api.MakeKey(this.sesid, this.tableid, Any.Guid, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a DateTime.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyDateTime()
        {
            this.CreateIndexOnColumn("DateTime");
            Api.MakeKey(this.sesid, this.tableid, DateTime.Now, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyUnicode()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.Unicode, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyASCII()
        {
            this.CreateIndexOnColumn("ascii");
            Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.ASCII, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Making a key with an invalid encoding throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyMakeKeyWithInvalidEncodingThrowsException()
        {
            this.CreateIndexOnColumn("unicode");

            try
            {
                Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.UTF32, MakeKeyGrbit.NewKey);
                Assert.Fail("Expected an EsentException");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /// <summary>
        /// Test make a key from an empty string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyEmptyString()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, string.Empty, Encoding.Unicode, MakeKeyGrbit.NewKey | MakeKeyGrbit.KeyDataZeroLength);
        }

        /// <summary>
        /// Test make a key from a string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyNullString()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, null, Encoding.Unicode, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from an array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, Any.Bytes, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a null array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyNullBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, null, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a zero-length array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MakeKeyZeroLengthBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, new byte[0], MakeKeyGrbit.NewKey);
        }

        #endregion MakeKey Tests

        #region MetaData helpers tests

        /// <summary>
        /// Test the helper method that gets table names.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetTableNames()
        {
            string actual = Api.GetTableNames(this.sesid, this.dbid).Single();
            Assert.AreEqual(this.table, actual);
        }

        /// <summary>
        /// Search the column information structures with Linq.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SearchColumnInfos()
        {
            IEnumerable<string> columnnames = from c in Api.GetTableColumns(this.sesid, this.tableid)
                             where c.Coltyp == JET_coltyp.Long
                             select c.Name;
            Assert.AreEqual("Int32", columnnames.Single());
        }

        /// <summary>
        /// Iterate through the column information structures.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetTableColumnsFromTableidTest()
        {
            foreach (ColumnInfo col in Api.GetTableColumns(this.sesid, this.tableid))
            {
                Assert.AreEqual(this.columnidDict[col.Name], col.Columnid);
            }
        }

        /// <summary>
        /// Iterate through the column information structures, using
        /// the dbid and tablename to specify the table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetTableColumnsByTableNameTest()
        {
            foreach (ColumnInfo col in Api.GetTableColumns(this.sesid, this.dbid, this.table))
            {
                Assert.AreEqual(this.columnidDict[col.Name], col.Columnid);
            }
        }

        /// <summary>
        /// Get index information when there are no indexes on the table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetIndexInformationNoIndexes()
        {
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);
            Assert.AreEqual(0, indexes.Count());
        }

        /// <summary>
        /// Get index information for one index
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetIndexInformationOneIndex()
        {
            string indexname = "myindex";
            string indexdef = "+ascii\0\0";
            CreateIndexGrbit grbit = CreateIndexGrbit.IndexUnique;

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, grbit, indexdef, indexdef.Length, 100);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(indexname, info.Name);
            Assert.AreEqual(grbit, info.Grbit);

            Assert.AreEqual(1, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("ascii", info.IndexSegments[0].ColumnName, true));
            Assert.IsTrue(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsTrue(info.IndexSegments[0].IsASCII);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Get index information for one index
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetIndexInformationOneIndexMultipleSegments()
        {
            string indexname = "multisegmentindex";
            string indexdef = "+ascii\0-boolean\0\0";
            CreateIndexGrbit grbit = CreateIndexGrbit.IndexUnique;

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, grbit, indexdef, indexdef.Length, 100);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(indexname, info.Name);
            Assert.AreEqual(grbit, info.Grbit);

            Assert.AreEqual(2, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("ascii", info.IndexSegments[0].ColumnName, true));
            Assert.IsTrue(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsTrue(info.IndexSegments[0].IsASCII);

            Assert.IsTrue(0 == string.Compare("boolean", info.IndexSegments[1].ColumnName, true));
            Assert.IsFalse(info.IndexSegments[1].IsAscending);
            Assert.AreEqual(JET_coltyp.Bit, info.IndexSegments[1].Coltyp);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Get index information for one index
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetIndexInformationByTableNameOneIndex()
        {
            string indexname = "myindex";
            string indexdef = "+ascii\0\0";
            CreateIndexGrbit grbit = CreateIndexGrbit.IndexUnique;

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, grbit, indexdef, indexdef.Length, 100);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.dbid, this.table);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(indexname, info.Name);
            Assert.AreEqual(grbit, info.Grbit);

            Assert.AreEqual(1, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("ascii", info.IndexSegments[0].ColumnName, true));
            Assert.IsTrue(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsTrue(info.IndexSegments[0].IsASCII);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Get index information for one index
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GetIndexInformationOneIndexWithCompareOptions()
        {
            const string Indexname = "myindex";
            const string Indexdef = "-unicode\0\0";

            var pidxUnicode = new JET_UNICODEINDEX
            {
                lcid = CultureInfo.CurrentCulture.LCID,
                dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase),
            };

            var indexcreate = new JET_INDEXCREATE
            {
                szIndexName = Indexname,
                szKey = Indexdef,
                cbKey = Indexdef.Length,
                grbit = CreateIndexGrbit.IndexDisallowNull,
                pidxUnicode = pidxUnicode,
            };

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex2(this.sesid, this.tableid, new[] { indexcreate }, 1);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(Indexname, info.Name);
            Assert.AreEqual(CreateIndexGrbit.IndexDisallowNull, info.Grbit);

            Assert.AreEqual(1, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("unicode", info.IndexSegments[0].ColumnName, true));
            Assert.IsFalse(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsFalse(info.IndexSegments[0].IsASCII);
            Assert.AreEqual(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase, info.CompareOptions);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        #endregion MetaData helpers tests

        #region Helper methods

        /// <summary>
        /// Creates a record with the given column set to the specified value.
        /// The tableid is positioned on the new record.
        /// </summary>
        /// <param name="columnid">The column to set.</param>
        /// <param name="data">The data to set.</param>
        private void InsertRecord(JET_COLUMNID columnid, byte[] data)
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetSetColumn(this.sesid, this.tableid, columnid, data, (null == data) ? 0 : data.Length, SetColumnGrbit.None, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);      
        }

        /// <summary>
        /// Creates a record with the given column set to the specified value.
        /// The tableid is positioned on the new record.
        /// </summary>
        /// <param name="columnid">The column to set.</param>
        /// <param name="values">The data to set.</param>
        private void InsertRecordWithSetColumns(JET_COLUMNID columnid, params ColumnValue[] values)
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumns(this.sesid, this.tableid, values);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Test setting and retrieving a null column.
        /// </summary>
        /// <typeparam name="T">The struct type that is being returned.</typeparam>
        /// <param name="column">The name of the column to set.</param>
        /// <param name="retrieveFunc">The function to use when retrieving the column.</param>
        private void NullColumnTest<T>(string column, Func<JET_SESID, JET_TABLEID, JET_COLUMNID, T?> retrieveFunc) where T : struct
        {
            JET_COLUMNID columnid = this.columnidDict[column];
            this.InsertRecord(columnid, null);
            Assert.IsNull(retrieveFunc(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            var bookmark = new byte[256];
            int bookmarkSize;
            Api.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        /// <summary>
        /// Create an ascending index over the given column. The tableid will be
        /// positioned to the new index.
        /// </summary>
        /// <param name="column">The name of the column to create the index on.</param>
        private void CreateIndexOnColumn(string column)
        {
            string indexname = String.Format("index_{0}", column);
            string indexdef = String.Format("+{0}\0\0", column);

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, CreateIndexGrbit.None, indexdef, indexdef.Length, 100);
            Api.JetSetCurrentIndex(this.sesid, this.tableid, indexname);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
        }

        #endregion Helper methods
    }
}
