// Copyright The ORAS Authors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using OrasProject.Oras.Content;
using OrasProject.Oras.Exceptions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OrasProject.Oras.Tests.Content
{
    public class LimitedReadStreamTest
    {
        [Fact]
        public void Read_WithinLimit_Succeeds()
        {
            var data = Encoding.UTF8.GetBytes("foobar");
            using var inner = new MemoryStream(data);
            using var limited = new LimitedReadStream(inner, 6);

            var buffer = new byte[6];
            int read = limited.Read(buffer, 0, buffer.Length);

            Assert.Equal(6, read);
            Assert.Equal("foobar", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public void Read_ExceedsLimit_Throws()
        {
            var data = Encoding.UTF8.GetBytes("foobar");
            using var inner = new MemoryStream(data);
            using var limited = new LimitedReadStream(inner, 6);

            var buffer = new byte[3];
            int read = limited.Read(buffer, 0, buffer.Length); // succeeds
            Assert.Equal(3, read);
            Assert.Equal("foo", Encoding.UTF8.GetString(buffer));

            read = limited.Read(buffer, 0, buffer.Length); // succeeds
            Assert.Equal(3, read);
            Assert.Equal("bar", Encoding.UTF8.GetString(buffer));

            Assert.Throws<SizeLimitExceededException>(() =>
            {
                int read = limited.Read(buffer, 0, 1);
            });
        }

        [Fact]
        public async Task ReadAsync_WithinLimit_Succeeds()
        {
            var data = Encoding.UTF8.GetBytes("foobar");
            using var inner = new MemoryStream(data);
            using var limited = new LimitedReadStream(inner, 6);

            var buffer = new byte[6];
            int read = await limited.ReadAsync(buffer);

            Assert.Equal(6, read);
            Assert.Equal("foobar", Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public async Task ReadAsync_ExceedsLimit_Throws()
        {
            var data = Encoding.UTF8.GetBytes("foobar");
            using var inner = new MemoryStream(data);
            using var limited = new LimitedReadStream(inner, 6);

            var buffer = new byte[3];
            int read = await limited.ReadAsync(buffer); // succeeds
            Assert.Equal(3, read);
            Assert.Equal("foo", Encoding.UTF8.GetString(buffer));

            read = await limited.ReadAsync(buffer); // succeeds
            Assert.Equal(3, read);
            Assert.Equal("bar", Encoding.UTF8.GetString(buffer));

            await Assert.ThrowsAsync<SizeLimitExceededException>(async () =>
            {
                _ = await limited.ReadAsync(buffer);
            });
        }

        [Fact]
        public void Write_Throws_NotSupported()
        {
            using var inner = new MemoryStream();
            using var limited = new LimitedReadStream(inner, 10);

            Assert.False(limited.CanWrite);
            var buffer = Encoding.UTF8.GetBytes("abc");
            Assert.Throws<NotSupportedException>(() => limited.Write(buffer, 0, buffer.Length));
        }

        [Fact]
        public void Seek_UpdatesBytesRead()
        {
            var data = Encoding.UTF8.GetBytes("abcdef");
            using var inner = new MemoryStream(data);
            using var limited = new LimitedReadStream(inner, 4);

            limited.Seek(2, SeekOrigin.Begin);
            var buffer = new byte[2];
            int read = limited.Read(buffer, 0, 2);

            Assert.Equal(2, read);
            Assert.Equal("cd", Encoding.UTF8.GetString(buffer));
        }
    }
}
