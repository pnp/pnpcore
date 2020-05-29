# Writing test cases

Test cases are a crucial part of the pnp core sdk as they help ensuring code quality and provide feedback to contributors on how well their changes are working. All new contributions **must** be convered by test cases unless the added functionality is already somehow covered by existing test cases.

## Setting up your environment to run the tests cases

If you want to use and extend these unit tests then you'll need to do a simple onetime setup:

1. Go to the test project folder (src\sdk\PnP.Core.Test)
2. Copy `appsettings.copyme.json` to `appsettings.xxx.json` (xxx identifies your test environment) and update the url's and accounts to match with what's available in your tenant. The test system requires that you have setup the following sites (optionally use `setuptestenv.ps1` to help create the needed sites):

   1. A modern, group connected, team site (recommended name is **pnpcoresdktestgroup**) which was **teamified** and which **has a sub site** (recommended name is **subsite**)
   2. A modern communication site (recommended name is **pnpcoresdktest**) which **has a sub site** (recommended name is **subsite**)

3. Copy env.sample to env.txt
4. Open env.txt and put as content the value **xxx** (xxx identifies your test environment)
5. That should be it. Happy testing!

## Running the existing tests in offline mode

The test model runs tests by default in an offline modus. This means that tests run really quick and that they should always work, even if you are offline (not connected to the Internet network). There are just few tests that forcibly require to be executed online, and those will fail when you are offline. After you've setup your environment for testing you should open the Visual Studio Test Explorer and click on the **Run all tests** button to verify that all tests run successfully on your computer.

![Test explorer](../../images/test%20explorer.png)

## Authoring new test cases

### Where do I put my test case?

All test cases belong to the PnP.Core.Test project and generally speaking the test cases will either be linked to extending the model or linked to the "core" part that handles the interaction with SharePoint or Microsoft Graph. For model related test cases, please add them to existing test classes in the respective model folders:

- SharePoint model tests go into the **SharePoint** folder. You either create a new file with test cases or add your test into an existing file.
- Teams model tests go into the **Teams** folder. You either create a new file with test cases or add your test into an existing file.
- Azure Active Directory model tests go into the **AzureActiveDirectory** folder. You either create a new file with test cases or add your test into an existing file

If your test extends an already tested model then most likely you'll be able to add your test to one of the existing test classes.

When you add "core" tests these will need to be added in the **Base** folder for core tests or in the **QueryModel** folder for linq tests.

### Anotomy of a typical test file and test case

It's easier to learn this by looking at a sample:

```csharp
[TestClass]
public class GetTests
{
    [ClassInitialize]
    public static void TestFixtureSetup(TestContext context)
    {
        // Configure mocking default for all tests in this class, unless override by a specific test
        //TestCommon.Instance.Mocking = false;
    }

    #region Tests that use REST to hit SharePoint

    [TestMethod]
    public async Task GetSinglePropertyViaRest()
    {
        //TestCommon.Instance.Mocking = false;
        using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
        {
            var web = await context.Web.GetAsync(p => p.WelcomePage);

            // Is the property populated
            Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
            Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));

            // Are other properties still not available
            Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
        }
    }
}
```

All test classes will have a `TestFixtureSetup` static method which is marked as class initializer. Using this method you can turn all tests in a given class from offline testing to online testing (so testing without using the mock data). Looking at the test method itself you'll notice that:

- To create a `PnPContext` the test cases uses the `TestCommon.Instance.GetContext` method: it's important to use this method as this one is hooked up to the configuration you specified plus it will deliver a context that understands how to work with mock data.
- Uncommenting the `TestCommon.Instance.Mocking = false;` line can be used to put this specific test in online mode: when you start developing your test you'll have do that (as you initially don't have mock data)

### Mocking of server responses in tests

The PnP Core SDK tests use a custom built mocking system which essentially simply saves server responses when running in online mode. When running in offline mode (so when mocking data) the saved server responses are used to mock the server response. All of this works automatically and can be turned on/off by setting the `TestCommon.Instance.Mocking` property to `true` or `false` (see also the FAQ at the end of this page to learn more). When running in online mode each response for a server request will be stored as a file in a sub folder of the current test class. This folder is always named `MockData` and has a sub folder per test class, which ensures that mocking files for a given test can be easily identified. The filename uses the following pattern `{testname}-{context order}-{sequence}.response`.

### Steps to create a test case

If you follow below steps you'll be creating test cases according to the PnP Core SDK guidelines:

1. Find the right location to add your test, either use an existing file or create a new test file
2. Add below test to get started

```csharp
    [TestMethod]
    public async Task NameOfYourTestMethod()
    {
        TestCommon.Instance.Mocking = false;
        using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
        {
            // here comes your actual test code
        }
    }
```

3. Write your test with mocking turned off
4. Once your test is ready delete the generated mock data files (see the `.response` files in the `MockData` folder) and run your test once more to regenerate them. This step ensures that no stale mocking files will be checked in.
5. Turn mocking on (commenting the `TestCommon.Instance.Mocking = false;` line) and verify your test still works
6. Check in your test case **together with** the offline files generated in the **mockdata** folder
7. **Optionally**: if your test is creating artifacts in Microsoft 365 it's best to code the cleanup in the test or in the default `cleantestenv.copyme.ps1` script

> [!Important]
> Each checked in test must be checked in with mocking turned on and as such with the appropiate offline test files (the `.response` files). This is important as it ensures that test cases execute really fast and that tests can be used in build/deploy pipelines.

## Frequently Asked Questions

### Do I need to recreate the sites after each live test run?

You can opt to recreate the sites each time, but that will be time consuming. It's better to clean the created artifacts before launching a new live test run. The artifacts to clean obviously depend on the written test cases and it's a best practice to keep the `cleantestenv.copyme.ps1` script up to date with the needed cleaning work. You can tailor this script for your environment and save it as `cleantestenv.xxx.ps1` (xxx identifies your test environment) to add your specific cleanup instructions.

### How can I configure tests to run live versus the default offline run?

By default all the tests run based upon offline data, this is done to enable fast test execution without being dependent on a Microsoft 365 backend to be available and configured for testing. If you however are adding new test cases or refactoring code you might want to run one or more tests in live mode. There are 3 options:

#### Run a single test live

To run a single test live you simply need to uncomment the `TestCommon.Instance.Mocking = false;` line to configure the test to run without using the mocking data, so run live.

```csharp
[TestMethod]
public async Task GetSinglePropertyViaRest()
{
    TestCommon.Instance.Mocking = false;
    using (var context = TestCommon.Instance.GetContext(TestCommon.TestSite))
    {
        var web = await context.Web.GetAsync(p => p.WelcomePage);

        // Is the property populated
        Assert.IsTrue(web.IsPropertyAvailable(p => p.WelcomePage));
        Assert.IsTrue(!string.IsNullOrEmpty(web.WelcomePage));

        // Are other properties still not available
        Assert.IsFalse(web.IsPropertyAvailable(p => p.Title));
    }
}
```

> [!Note]
> Please only commit tests that can run offline: tests need to have offline data + mocking must be turned on.

#### Run all the tests in a test class live

To run all tests in a given test class live you can turn off mocking via a test class initialized:

```csharp
[ClassInitialize]
public static void TestFixtureSetup(TestContext context)
{
    // Configure mocking default for all tests in this class, unless override by a specific test
    TestCommon.Instance.Mocking = false;
}
```

> [!Note]
> Please only commit tests that can run offline: tests need to have offline data + mocking must be turned on.

#### Run all the tests live

In the rare case you want to run all the test cases live you need to change the default setting of the `Mocking` property from true to false in TestCommon.cs (in the Utilities folder):

```csharp
public bool Mocking { get; set; } = false;
```

> [!Note]
> Please only commit tests that can run offline: tests need to have offline data + mocking must be turned on.

### Can I rename test files or test cases

Yes, if naming needs to change you can do that. Keep in mind that the offline files live in a folder named accordingly to the test file and the offline files depend on the test case name, so you'll have to do similar renames in offline files or folder.

### My test cannot in offline mode and as such it breaks the GitHub "Build and Test" workflow

If you really can't make your test work offline then you'll need to exclude the test from being executed in the GitHub **Build and Test** workflow. This can be done by adding the following check to your test case:

```csharp
if (TestCommon.RunningInGitHubWorkflow()) Assert.Inconclusive("Skipping live test because we're running inside a GitHub action");
```
