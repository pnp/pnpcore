# Using the unit tests

## Quick starts

### I want to run the unit tests

If you want (and you should) use and extend these unit tests then:

1. Copy env.sample to env.txt
2. Open env.txt and put as content the value **mine** (or another name in case you want to use other test environment names)
3. Open `appsettings.mine.json` and update the url's and accounts to match with what's available in your tenant. The test system requires that you have setup the following sites (optionally use `setuptestenv.ps1` to help create the needed sites):

   1. A modern, group connected, team site (recommended name is **pnpcoresdktestgroup**) which was teamified and which has a sub site (recommended name is **subsite**)
   2. A modern communication site (recommended name is **pnpcoresdktest**) which has a sub site (recommended name is **subsite**)

4. Happy testing!

## FAQ

### Do I need to recreate the sites after each live test run?

You can opt to recreate the sites each time, but that will be time consuming. It's better to clean the created artefacts before launching a new live test run. The artefacts to clean obviously depend on the written test cases and it's a best practice to keep the `cleantestenv.ps1` script up to date with the needed cleaning work. You can tailor this script and save it as `cleantestenv.mine.ps1` to add your specific cleanup instructions.

### How can I configure tests to run live versus the default offline run?

By default all the tests run based upon offline data, this is done to enable fast test execution without being dependent on a Microsoft 365 backend to be available and configured for testing. If you however are adding new test cases or refactoring code you might want to run one or more tests in live mode. There are 3 options:

#### Run a single test live

To run a single test live you simply need to uncomment the `TestCommon.Instance.Mocking = false;` line to to configure the test to run without using the mocking data, so run live.

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

> Note:
> Please only commit tests that can run offline: tests need to have offline data + mocking must be turned on

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

> Note:
> Please only commit tests that can run offline: tests need to have offline data + mocking must be turned on

#### Run all the tests live

In the rare case you want to run all the test cases live you need to change the default setting of the `Mocking` property from true to false in TestCommon.cs (in the Utilities folder):

```csharp
public bool Mocking { get; set; } = false;
```

> Note:
> Please only commit tests that can run offline: tests need to have offline data + mocking must be turned on
