const core = require("@actions/core");
const { execSync } = require("child_process");
const fs = require("fs");
const path = require("path");

async function main() {
  try {
    const packagePath = core.getInput("package-path", { required: true });
    const orchestratorUrl = core.getInput("orchestrator-url", {
      required: true,
    });
    const orchestratorTenant = core.getInput("orchestrator-tenant", {
      required: true,
    });
    const orchestratorFolder = core.getInput("orchestrator-folder") || "Shared";
    const accountName = core.getInput("account-name");
    const applicationId = core.getInput("application-id");
    const applicationSecret = core.getInput("application-secret");
    const applicationScope = core.getInput("application-scope");

    core.info(`Publishing package to UiPath Orchestrator: ${packagePath}`);

    // Verify package exists
    if (!fs.existsSync(packagePath)) {
      throw new Error(`Package file or folder not found: ${packagePath}`);
    }

    // Validate authentication parameters for app-based auth
    if (!applicationId || !applicationSecret) {
      throw new Error(
        "Application credentials required: provide application-id and application-secret"
      );
    }

    // Set up authentication using environment variables and config
    const publishEnv = {
      ...process.env,
      UIPATH_URI: orchestratorUrl,
      UIPATH_ORGANIZATION: accountName,
      UIPATH_TENANT: orchestratorTenant,
    };

    // Configure authentication using the new CLI approach
    core.info("Configuring UiPath CLI authentication...");
    
    // Set auth type to credentials (external app)
    execSync(`uipath config set --key auth.grantType --value client_credentials`, {
      stdio: 'inherit',
      env: publishEnv
    });

    // Set application ID
    execSync(`uipath config set --key auth.properties.clientId --value "${applicationId}"`, {
      stdio: 'inherit',
      env: publishEnv
    });

    // Set application secret
    execSync(`uipath config set --key auth.properties.clientSecret --value "${applicationSecret}"`, {
      stdio: 'inherit',
      env: publishEnv
    });

    // Set scopes if provided
    if (applicationScope) {
      execSync(`uipath config set --key auth.scopes --value "${applicationScope}"`, {
        stdio: 'inherit',
        env: publishEnv
      });
    }

    // Build the modern uipath command
    const publishCommand = [
      "uipath",
      "studio",
      "package", 
      "publish",
      "--source", `"${packagePath}"`,
      "--folder", `"${orchestratorFolder}"`,
      "--uri", `"${orchestratorUrl}"`,
      "--organization", `"${accountName}"`,
      "--tenant", `"${orchestratorTenant}"`
    ];

    const deployCommand = publishCommand.join(" ");

    core.info("Publishing to UiPath Orchestrator...");
    core.info(`Command: ${deployCommand}`);

    try {
      execSync(deployCommand, {
        stdio: "inherit",
        env: publishEnv,
      });
      core.info("Package published successfully to UiPath Orchestrator");
    } catch (error) {
      // Check for specific error patterns that might be expected
      if (error.message.includes("already exists")) {
        core.warning(
          "Package already exists in Orchestrator, but deployment completed"
        );
      } else {
        throw error;
      }
    }

    // Add summary
    const packageName = path.basename(packagePath);
    await core.summary
      .addHeading("Package Published to UiPath Orchestrator")
      .addTable([
        [
          { data: "Property", header: true },
          { data: "Value", header: true },
        ],
        ["Package", packageName],
        ["Orchestrator URL", orchestratorUrl],
        ["Organization", accountName],
        ["Tenant", orchestratorTenant],
        ["Folder", orchestratorFolder],
        ["Status", "Successfully published"],
      ])
      .write();
  } catch (error) {
    core.setFailed(
      `Failed to publish to UiPath Orchestrator: ${error.message}`
    );
    core.error(error.stack || error.toString());
  }
}

if (require.main === module) {
  main();
}

module.exports = { main };
