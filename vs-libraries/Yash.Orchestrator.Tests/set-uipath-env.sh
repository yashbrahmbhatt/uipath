#!/bin/bash

echo "Setting UiPath test environment variables..."

read -p "UIP_ACCOUNT_NAME: " UIP_ACCOUNT_NAME
read -p "UIP_APPLICATION_ID: " UIP_APPLICATION_ID
read -p "UIP_APPLICATION_SECRET: " UIP_APPLICATION_SECRET
read -p "UIP_TENANT_NAME: " UIP_TENANT_NAME
read -p "UIP_TEST_SCOPES [default: OR.Assets.Read OR.Folders.Read]: " UIP_TEST_SCOPES

set UIP_ACCOUNT_NAME="${UIP_ACCOUNT_NAME}"
set UIP_APPLICATION_ID="${UIP_APPLICATION_ID}"
set UIP_APPLICATION_SECRET="${UIP_APPLICATION_SECRET}"
set UIP_TENANT_NAME="${UIP_TENANT_NAME}"
set UIP_TEST_SCOPES="${UIP_TEST_SCOPES:-OR.Assets.Read OR.Folders.Read}"

echo "UiPath environment variables set for this session."
