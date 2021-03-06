﻿//-----------------------------------------------------------------------
// <copyright file="SystemParameterTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Implementation;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Mocks;

namespace InteropApiTests
{
    /// <summary>
    /// Test the SystemParameters class. To avoid changing global parameters
    /// this is tested with a mock IJetApi.
    /// </summary>
    [TestClass]
    public class SystemParameterTests
    {
        /// <summary>
        /// Mock object repository.
        /// </summary>
        private MockRepository repository;

        /// <summary>
        /// The real IJetApi, saved in Setup and restored in Teardown.
        /// </summary>
        private IJetApi savedApi;

        /// <summary>
        /// Mock API object.
        /// </summary>
        private IJetApi mockApi;

        /// <summary>
        /// Initialization method. Setup the mock API.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.savedApi = Api.Impl;
            this.repository = new MockRepository();
            this.mockApi = this.repository.DynamicMock<IJetApi>();

            var mockCapabilities = new JetCapabilities
                {
                    SupportsLargeKeys = true,
                    SupportsUnicodePaths = true,
                    SupportsVistaFeatures = true,
                    SupportsWindows7Features = true,
                };
            SetupResult.For(this.mockApi.Capabilities).Return(mockCapabilities);

            Api.Impl = this.mockApi;
        }

        /// <summary>
        /// Cleanup after a test. This restores the saved API.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.Impl = this.savedApi;
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingCacheSizeMax()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMax, 64, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.CacheSizeMax = 64;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingCacheSize()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSize, 64, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.CacheSize = 64;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingCacheSizeMin()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, 64, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.CacheSizeMin = 64;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingDatabasePageSize()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.DatabasePageSize, 4096, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.DatabasePageSize = 4096;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingMaxInstances()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.MaxInstances, 12, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.MaxInstances = 12;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingConfiguration()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, VistaParam.Configuration, 0, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.Configuration = 0;
            this.repository.VerifyAll();
        }

        /// <summary>
        /// Verify that setting the property sets the system parameter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifySettingEnableAdvanced()
        {
            Expect.Call(
                this.mockApi.JetSetSystemParameter(
                    JET_INSTANCE.Nil, JET_SESID.Nil, VistaParam.EnableAdvanced, 1, null)).Return(1);
            this.repository.ReplayAll();
            SystemParameters.EnableAdvanced = true;
            this.repository.VerifyAll();
        }
    }
}
