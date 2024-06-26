﻿using System;
using System.IO;
using Spectre.Console;

namespace AElfChain.Common.Helpers
{
    public class SmartContractReader
    {
        private const string ContractExtension = ".dll";
        private const string ContractPatchedExtension = ".dll";
        private const string ContractFolderName = "contracts";

        private readonly string _dataDirectory;

        public SmartContractReader(string dic = "")
        {
            _dataDirectory = dic == ""
                ? CommonHelper.GetDefaultDataDir()
                : dic;
        }

        public byte[] Read(string name)
        {
            try
            {
                byte[] code;
                using (var file = File.OpenRead(GetKeyFileFullPath(name)))
                {
                    code = file.GetAllBytes();
                }

                return code;
            }
            catch (Exception e)
            {
                AnsiConsole.WriteLine("SmartContractReader: Invalid transaction data.");
                AnsiConsole.WriteLine($"Exception: {e.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Return the full path of the files
        /// </summary>
        private string GetKeyFileFullPath(string contractName)
        {
            var dirPath = GetKeystoreDirectoryPath();
            var filePath = Path.Combine(dirPath, contractName);

            if (contractName.Contains(ContractExtension))
                return contractName;

            var filePathWithExtension = filePath + ContractPatchedExtension;
            if (File.Exists(filePathWithExtension))
                return filePathWithExtension;

            filePathWithExtension = filePath + ContractExtension;

            return filePathWithExtension;
        }

        private string GetKeystoreDirectoryPath()
        {
            return Path.Combine(_dataDirectory, ContractFolderName);
        }
    }
}