using System;

namespace CleanGraphQLApi.Presentation.Tests.Integration;

using System.Net;
using System.Threading.Tasks;
using CleanGraphQLApi.Presentation.Tests.Integration.Models.Common;
using Shouldly;
using Xunit;

public class ReviewTests
{
    private static readonly GraphQLApplication Application = new();

    [Fact]
    public async Task Reviews_ShouldCreate_Review()
    {
        // Arrange
        using var client = Application.CreateClient();
        using var authorResponse = await client.GetAsync("/graphql?query={authors{id,firstName,lastName}}");
        var author = (await authorResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>().Data.Authors[0];
        using var movieResponse = await client.GetAsync("/graphql?query={movies{id,title}}");
        var movie = (await movieResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>().Data.Movies[0];

        // Act
        var content = new Models.Reviews.Create.CreateReviewInputData
        {
            Query = "mutation($review:CreateReviewInput!){createReview(input:$review){id,stars,dateCreated,dateModified,author{id,firstName,lastName,dateCreated,dateModified,},movie{id,title,dateCreated,dateModified}}}",
            Variables = new Models.Reviews.Create.Variables
            {
                Review = new Models.Reviews.Create.Review
                {
                    AuthorId = author.Id,
                    MovieId = movie.Id,
                    Stars = 5
                }
            }
        };
        using var response = await client.PostAsync($"/graphql", content.ToStringContent());
        var result = (await response.Content.ReadAsStringAsync()).Deserialize<GraphData>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _ = result.ShouldNotBeNull();
        _ = result.Data.ShouldNotBeNull();

        _ = result.Data.CreateReview.ShouldNotBeNull();
        _ = result.Data.CreateReview.Id.ShouldBeOfType<Guid>();
        _ = result.Data.CreateReview.Stars.ShouldBeOfType<int>();
        result.Data.CreateReview.Stars.ShouldBe(5);
        result.Data.CreateReview.DateCreated.ShouldBeOfType<DateTime>();
        result.Data.CreateReview.DateCreated.ShouldNotBe(default);
        result.Data.CreateReview.DateModified.ShouldBeOfType<DateTime>();
        result.Data.CreateReview.DateModified.ShouldNotBe(default);

        _ = result.Data.CreateReview.Author.ShouldNotBeNull();
        result.Data.CreateReview.Author.Id.ShouldBe(author.Id);
        result.Data.CreateReview.Author.FirstName.ShouldBe(author.FirstName);
        result.Data.CreateReview.Author.LastName.ShouldBe(author.LastName);
        result.Data.CreateReview.Author.DateCreated.ShouldBeOfType<DateTime>();
        result.Data.CreateReview.Author.DateCreated.ShouldNotBe(default);
        result.Data.CreateReview.Author.DateModified.ShouldBeOfType<DateTime>();
        result.Data.CreateReview.Author.DateModified.ShouldNotBe(default);

        _ = result.Data.CreateReview.Movie.ShouldNotBeNull();
        result.Data.CreateReview.Movie.Id.ShouldBe(movie.Id);
        result.Data.CreateReview.Movie.Title.ShouldBe(movie.Title);
        result.Data.CreateReview.Movie.DateCreated.ShouldBeOfType<DateTime>();
        result.Data.CreateReview.Movie.DateCreated.ShouldNotBe(default);
        result.Data.CreateReview.Movie.DateModified.ShouldBeOfType<DateTime>();
        result.Data.CreateReview.Movie.DateModified.ShouldNotBe(default);
    }

    [Fact]
    public async Task Reviews_ShouldDelete_Review()
    {
        // Arrange
        using var client = Application.CreateClient();
        using var reviewResponse = await client.GetAsync("/graphql?query={reviews{id}}");
        var reviewId = (await reviewResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>().Data.Reviews[0].Id;

        // Act
        var content = new Models.Reviews.Delete.DeleteReviewInputData
        {
            Query = "mutation($id:ID!){deleteReview(id:$id)}",
            Variables = new Models.Reviews.Delete.Variables
            {
                Id = reviewId
            }
        };
        using var response = await client.PostAsync($"/graphql", content.ToStringContent());
        var resultS = await response.Content.ReadAsStringAsync();
        var result = (await response.Content.ReadAsStringAsync()).Deserialize<GraphData>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _ = result.ShouldNotBeNull();
        _ = result.Data.ShouldNotBeNull();

        result.Data.DeleteReview.ShouldBeTrue();
    }

    [Fact]
    public async Task Reviews_ShouldReturn_ReviewsList()
    {
        // Arrange
        using var client = Application.CreateClient();

        // Act
        using var response = await client.GetAsync("/graphql?query={reviews{id,stars,dateCreated,dateModified,author{id,firstName,lastName,dateCreated,dateModified},movie{id,title,dateCreated,dateModified}}}");
        var result = (await response.Content.ReadAsStringAsync()).Deserialize<GraphData>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _ = result.ShouldNotBeNull();
        _ = result.Data.ShouldNotBeNull();

        result.Data.Reviews.ShouldNotBeEmpty();
        result.Data.Reviews.Length.ShouldBe(150);

        foreach (var review in result.Data.Reviews)
        {
            _ = review.ShouldNotBeNull();
            _ = review.Id.ShouldBeOfType<Guid>();
            _ = review.Stars.ShouldBeOfType<int>();
            review.Stars.ShouldBeInRange(1, 5);
            review.Author.DateCreated.ShouldBeOfType<DateTime>();
            review.Author.DateCreated.ShouldNotBe(default);
            review.Author.DateModified.ShouldBeOfType<DateTime>();
            review.Author.DateModified.ShouldNotBe(default);

            _ = review.Author.ShouldNotBeNull();
            _ = review.Author.Id.ShouldBeOfType<Guid>();
            _ = review.Author.FirstName.ShouldBeOfType<string>();
            review.Author.FirstName.ShouldNotBeNullOrWhiteSpace();
            _ = review.Author.LastName.ShouldBeOfType<string>();
            review.Author.LastName.ShouldNotBeNullOrWhiteSpace();
            review.Author.DateCreated.ShouldBeOfType<DateTime>();
            review.Author.DateCreated.ShouldNotBe(default);
            review.Author.DateModified.ShouldBeOfType<DateTime>();
            review.Author.DateModified.ShouldNotBe(default);

            _ = review.Movie.ShouldNotBeNull();
            _ = review.Movie.Id.ShouldBeOfType<Guid>();
            _ = review.Movie.Title.ShouldBeOfType<string>();
            review.Movie.Title.ShouldNotBeNullOrWhiteSpace();
            review.Movie.DateCreated.ShouldBeOfType<DateTime>();
            review.Movie.DateCreated.ShouldNotBe(default);
            review.Movie.DateModified.ShouldBeOfType<DateTime>();
            review.Movie.DateModified.ShouldNotBe(default);
        }
    }

    [Fact]
    public async Task Review_ShouldReturn_Review()
    {
        // Arrange
        using var client = Application.CreateClient();
        using var setupResponse = await client.GetAsync("/graphql?query={reviews{id}}");
        var setupResult = (await setupResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>();
        var reviewId = setupResult.Data.Reviews[0].Id;

        // Act
        using var response = await client.GetAsync($"/graphql?query={{review(id:\"{reviewId}\"){{id,stars,dateCreated,dateModified,author{{id,firstName,lastName,dateCreated,dateModified}},movie{{id,title,dateCreated,dateModified}}}}}}");
        var result = (await response.Content.ReadAsStringAsync()).Deserialize<GraphData>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _ = result.ShouldNotBeNull();
        _ = result.Data.ShouldNotBeNull();

        _ = result.Data.Review.ShouldNotBeNull();
        _ = result.Data.Review.Id.ShouldBeOfType<Guid>();
        _ = result.Data.Review.Stars.ShouldBeOfType<int>();
        result.Data.Review.Stars.ShouldBeInRange(1, 5);
        result.Data.Review.DateCreated.ShouldBeOfType<DateTime>();
        result.Data.Review.DateCreated.ShouldNotBe(default);
        result.Data.Review.DateModified.ShouldBeOfType<DateTime>();
        result.Data.Review.DateModified.ShouldNotBe(default);

        _ = result.Data.Review.Author.ShouldNotBeNull();
        _ = result.Data.Review.Author.Id.ShouldBeOfType<Guid>();
        _ = result.Data.Review.Author.FirstName.ShouldBeOfType<string>();
        result.Data.Review.Author.FirstName.ShouldNotBeNullOrWhiteSpace();
        _ = result.Data.Review.Author.LastName.ShouldBeOfType<string>();
        result.Data.Review.Author.LastName.ShouldNotBeNullOrWhiteSpace();
        result.Data.Review.Author.DateCreated.ShouldBeOfType<DateTime>();
        result.Data.Review.Author.DateCreated.ShouldNotBe(default);
        result.Data.Review.Author.DateModified.ShouldBeOfType<DateTime>();
        result.Data.Review.Author.DateModified.ShouldNotBe(default);

        _ = result.Data.Review.Movie.ShouldNotBeNull();
        _ = result.Data.Review.Movie.Id.ShouldBeOfType<Guid>();
        _ = result.Data.Review.Movie.Title.ShouldBeOfType<string>();
        result.Data.Review.Movie.DateCreated.ShouldBeOfType<DateTime>();
        result.Data.Review.Movie.DateCreated.ShouldNotBe(default);
        result.Data.Review.Movie.DateModified.ShouldBeOfType<DateTime>();
        result.Data.Review.Movie.DateModified.ShouldNotBe(default);
    }

        [Fact]
    public async Task Update_ShouldUpdate_Review()
    {
        // Arrange
        using var client = Application.CreateClient();
        using var authorResponse = await client.GetAsync("/graphql?query={authors{id}}");
        var author = (await authorResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>().Data.Authors[0];
        using var movieResponse = await client.GetAsync("/graphql?query={movies{id}}");
        var movie = (await movieResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>().Data.Movies[0];
        using var reviewResponse = await client.GetAsync("/graphql?query={reviews{id}}");
        var review = (await reviewResponse.Content.ReadAsStringAsync()).Deserialize<GraphData>().Data.Reviews[0];

        // Act
        var content = new Models.Reviews.Update.UpdateReviewInputData()
        {
            Query = "mutation($review:UpdateReviewInput!){updateReview(input:$review)}",
            Variables = new Models.Reviews.Update.Variables
            {
                Review = new Models.Reviews.Update.Review
                {
                    Id = review.Id,
                    AuthorId = author.Id,
                    MovieId = movie.Id,
                    Stars = 5
                }
            }
        };
        var stringContent = content.ToStringContent();
        using var response = await client.PostAsync($"/graphql", content.ToStringContent());
        var stringtest = await response.Content.ReadAsStringAsync();
        var result = (await response.Content.ReadAsStringAsync()).Deserialize<GraphData>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        _ = result.ShouldNotBeNull();
        _ = result.Data.ShouldNotBeNull();

        result.Data.UpdateReview.ShouldBeTrue();
    }
}
