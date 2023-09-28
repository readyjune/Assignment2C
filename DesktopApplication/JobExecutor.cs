using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models;

namespace DesktopApplication
{
    public class JobExecutor
    {
        public async Task ExecuteJobForClient(Client client)
        {
            // Example to fake a progress increment for a client
            await Task.Run(async () =>
            {
                while (client.ProgressValue < 100)
                {
                    await Task.Delay(1000); // Delay for 1 second
                    client.ProgressValue += 10;

                    if (client.ProgressValue >= 100)
                    {
                        client.ProgressValue = 100;
                        client.IsBusy = false;
                    }
                }
                client.ProgressValue = 0;
            });
        }
    }

}
