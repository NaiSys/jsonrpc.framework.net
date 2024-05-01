# jsonrpc.framework.net

A simple to use framework for json rpc requests

## Example

```
public partial class LoginPage : ContentPage
{
    private readonly jsonrpc.RPCClient rpc = new("http://192.168.1.66:8069/jsonrpc");
	public LoginPage()
	{
		InitializeComponent();
	}
 
    private void OnLoginClicked(object sender, EventArgs e)
    {
        _ = rpc.Invoke("call", new {
            service = "common",
            method = "login",
            args = new string[] { "bitnami", UnameField.Text, PasswordField.Text }
        }, onResponse);
    }

    private async void onResponse(jsonrpc.RPCResponse response)
    {
        Debug.WriteLine(response);

        if(response.Error != null)
        {
            Debug.WriteLine(response.Error);
            await DisplayAlert("RPC Error", response.Error.ToString(), "Cancel");
        }
    }

    private void ImageLoaded(object sender, EventArgs e)
    {
        Debug.WriteLine(ImageSplash);
    }
}
```
