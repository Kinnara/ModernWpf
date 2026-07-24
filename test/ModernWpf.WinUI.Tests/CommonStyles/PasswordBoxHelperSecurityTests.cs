using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class PasswordBoxHelperSecurityTests
{
    [TestMethod]
    public void PasswordBoxHelperDoesNotReadPlaintextForPasswordState()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(
                FindRepoRoot(),
                "ModernWpf",
                "Controls",
                "Primitives",
                "PasswordBoxHelper.cs"));

        Assert.AreEqual(
            2,
            System.Text.RegularExpressions.Regex.Matches(
                source,
                @"_passwordBox\.Password(?!Changed)").Count,
            "Only the intentional visible-mode getter and setter may access PasswordBox.Password.");
        Assert.IsTrue(
            source.Contains("using var password = _passwordBox.SecurePassword;", StringComparison.Ordinal),
            "Password state checks should use SecurePassword instead of creating plaintext strings.");
        Assert.IsTrue(
            source.Contains(
                "PasswordRevealMode != PasswordRevealMode.Hidden && _textBox.IsVisible",
                StringComparison.Ordinal),
            "The plaintext getter must remain guarded by actual reveal visibility.");
    }

    [TestMethod]
    public void RevealTextBoxOnlyContainsPasswordWhilePlaintextIsVisible()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var passwordBox = new PasswordBox
            {
                Template = CreateRevealTemplate(),
                Width = 240
            };
            PasswordBoxHelper.SetIsEnabled(passwordBox, true);

            using var host = new TestWindowHost(passwordBox, width: 320, height: 120);
            var revealTextBox = GetRevealTextBox(passwordBox);
            const string password = "issue-331-regression-value";

            passwordBox.Password = password;
            host.UpdateLayout();

            Assert.AreEqual(
                0,
                revealTextBox.Text.Length,
                "A collapsed reveal TextBox must not retain a managed plaintext password copy.");

            revealTextBox.Visibility = Visibility.Visible;
            host.UpdateLayout();

            Assert.IsTrue(
                string.Equals(password, revealTextBox.Text, StringComparison.Ordinal),
                "The password should be copied only while the reveal TextBox is visible.");

            revealTextBox.Visibility = Visibility.Collapsed;
            host.UpdateLayout();

            Assert.AreEqual(
                0,
                revealTextBox.Text.Length,
                "Hiding the reveal TextBox must clear its managed plaintext password copy.");

            PasswordBoxHelper.SetPasswordRevealMode(passwordBox, PasswordRevealMode.Hidden);
            revealTextBox.Visibility = Visibility.Visible;
            host.UpdateLayout();

            Assert.AreEqual(
                0,
                revealTextBox.Text.Length,
                "Hidden reveal mode must not populate a visible template TextBox.");

            PasswordBoxHelper.SetPasswordRevealMode(passwordBox, PasswordRevealMode.Visible);
            host.UpdateLayout();

            Assert.IsTrue(
                string.Equals(password, revealTextBox.Text, StringComparison.Ordinal),
                "Visible reveal mode should continue to support intentional plaintext display.");

            const string updatedPassword = "issue-331-updated-value";
            revealTextBox.Text = updatedPassword;
            host.UpdateLayout();

            Assert.IsTrue(
                string.Equals(updatedPassword, passwordBox.Password, StringComparison.Ordinal),
                "Editing in visible reveal mode should continue to update the PasswordBox.");

            PasswordBoxHelper.SetIsEnabled(passwordBox, false);

            Assert.AreEqual(
                0,
                revealTextBox.Text.Length,
                "Detaching the helper must clear any remaining managed plaintext password copy.");

            using var securePassword = passwordBox.SecurePassword;
            Assert.AreEqual(
                updatedPassword.Length,
                securePassword.Length,
                "Detaching the helper must not clear the PasswordBox's secure value.");
        });
    }

    private static TextBox GetRevealTextBox(PasswordBox passwordBox)
    {
        return passwordBox.Template.FindName("TextBox", passwordBox) as TextBox
            ?? throw new AssertFailedException("Expected the test template to contain the reveal TextBox.");
    }

    private static ControlTemplate CreateRevealTemplate()
    {
        const string xaml = """
            <ControlTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                TargetType="{x:Type PasswordBox}">
                <Grid>
                    <ScrollViewer x:Name="PART_ContentHost" />
                    <TextBox x:Name="TextBox" Visibility="Collapsed" />
                </Grid>
            </ControlTemplate>
            """;

        return (ControlTemplate)XamlReader.Parse(xaml);
    }

    private static string FindRepoRoot()
    {
        var directory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
