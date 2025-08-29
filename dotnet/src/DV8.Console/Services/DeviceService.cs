using DV8.Console.Data;
using Microsoft.EntityFrameworkCore;

namespace DV8.Console.Services
{
    public class DeviceService
    {
        private readonly DV8DbContext _context;

        public DeviceService(DV8DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Device>> GetAllDevicesAsync()
        {
            return await _context.Devices
                .Include(d => d.Interfaces)
                .Include(d => d.SecretFields)
                .ToListAsync();
        }

        public async Task<Device?> GetDeviceByIdAsync(Guid id)
        {
            return await _context.Devices
                .Include(d => d.Interfaces)
                .Include(d => d.SecretFields)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Device?> GetDeviceByFingerprintAsync(string fingerprint)
        {
            return await _context.Devices
                .Include(d => d.Interfaces)
                .FirstOrDefaultAsync(d => d.Fingerprint == fingerprint);
        }

        public async Task<Device> CreateDeviceAsync(Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<Device?> UpdateDeviceAsync(Guid id, Device device)
        {
            var existingDevice = await _context.Devices.FindAsync(id);
            if (existingDevice == null) return null;

            existingDevice.Tenant = device.Tenant;
            existingDevice.Kind = device.Kind;
            existingDevice.Vendor = device.Vendor;
            existingDevice.Model = device.Model;
            existingDevice.SwVersion = device.SwVersion;
            existingDevice.Site = device.Site;
            existingDevice.Posture = device.Posture;
            existingDevice.TrustState = device.TrustState;

            await _context.SaveChangesAsync();
            return existingDevice;
        }

        public async Task<bool> DeleteDeviceAsync(Guid id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return false;

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Device>> GetDevicesByTenantAsync(string tenant)
        {
            return await _context.Devices
                .Where(d => d.Tenant == tenant)
                .Include(d => d.Interfaces)
                .ToListAsync();
        }

        public async Task<IEnumerable<Device>> GetDevicesBySiteAsync(string site)
        {
            return await _context.Devices
                .Where(d => d.Site == site)
                .Include(d => d.Interfaces)
                .ToListAsync();
        }
    }
}